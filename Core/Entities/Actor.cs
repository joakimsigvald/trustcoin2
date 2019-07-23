using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Actor : IActor
    {
        private readonly INetwork _network;
        private readonly ITransactionFactory _transactionFactory;
        public IAccount Account { get; }

        public Actor(INetwork network, IAccount account, ITransactionFactory transactionFactory)
        {
            _network = network;
            Account = account;
            _transactionFactory = transactionFactory;
        }

        public IPeer ProducePeer(AgentId id)
            => Account.IsConnectedTo(id) ? Account.GetPeer(id) : Connect(id);

        public IPeer Connect(AgentId id)
        {
            if (Account.IsConnectedTo(id))
                return Account.GetPeer(id);
            var newPeer = _network.FindAgent(id).AsPeer();
            Account.AddPeer(newPeer);
            SyncPeer(newPeer);
            OnAddedConnection(id);
            return newPeer;
        }

        public IAccount CreateAccount(string name)
        {
            var child = Account.CreateChild(name);
            _network.AddAccount(child);
            OnChildCreated(new NewAgent(child));
            var actor = _network.CreateActor(child);
            actor.Connect(Account.Id);
            return child;
        }

        public void Endorce(AgentId id)
        {
            ProducePeer(id).Endorce();
            OnEndorcedAgent(id);
        }

        public void EndorceArtefact(Artefact artefact)
        {
            if (artefact.OwnerId == default)
                throw new ArgumentException("Tried to endorce artefact without owner");
            var owner = ProducePeer(artefact.OwnerId);
            if (Account.KnowsArtefact(artefact.Id))
            {
                artefact = Account.GetArtefact(artefact.Id);
                if (owner.Id != artefact.OwnerId)
                    throw new ArgumentException($"Tried to endorce artefact with invalid owner: {owner.Id}");
            }
            else
                Account.RememberArtefact(artefact);
            Account.IncreaseTrust(owner.Id, ArtefactEndorcementTrustFactor);
            OnEndorcedArtefact(artefact);
        }

        public void RenewKeys()
        {
            var oldKey = Account.PublicKey;
            Account.RenewKeys();
            OnRenewedKey(oldKey, Account.PublicKey);
        }

        public override string ToString() => Account.Name;

        public void SyncAll()
        {
            Sync(
                Account.Peers.Select(peer => peer.Id).ToArray(), 
                Account.Artefacts.Select(a => a.Id).ToArray());
        }

        public Artefact CreateArtefact(string name, bool isResilient = false)
        {
            var artefact = Account.CreateArtefact(name, isResilient);
            OnCreatedArtefact(artefact);
            return artefact;
        }

        public void CounterfeitArtefact(Artefact artefact)
        {
            Account.ForgetArtefact(artefact.Id);
            var counterfeit = new Artefact(artefact, Account.Id);
            OnCreatedArtefact(counterfeit);
        }

        public void DestroyArtefact(ArtefactId id)
        {
            if (!Account.KnowsArtefact(id) || Account.GetArtefact(id).OwnerId != Account.Id)
                throw new ArgumentException("Tried to destroy artefact not owned");
            OnDestroyArtefact(Account.GetArtefact(id));
        }

        public string StartTransaction(AgentId clientId, params Transfer[] transfers)
        {
            if (!Verify(transfers))
                return null;
            var transactionKey = _transactionFactory.CreateTransactionKey();
            var transaction = new Transaction
            {
                Key = transactionKey,
                Transfers = transfers
            };
            Account.AddPendingTransaction(transaction);
            SendTransaction(ProducePeer(clientId), transaction);
            return transactionKey;
        }

        public bool AcceptTransaction(string transactionKey)
        {
            if (!Account.HasPendingTransaction(transactionKey))
                return false;
            var transaction = Account.GetPendingTransaction(transactionKey);
            if (!VerifyTransaction(transaction))
                return false;
            Account.ClosePendingTransaction(transactionKey);
            OnTransactionAccepted(transaction);
            return true;
        }

        public void RelayTransactionAccepted(Transaction transaction)
        {
            OnTransactionAccepted(transaction);
            Account.ClosePendingTransaction(transaction.Key);
        }

        public IHolder GetPeerAssessment(AgentId id, params AgentId[] asking)
            => Account.IsConnectedTo(id) ? Account.GetPeer(id)
            : asking.Length <= SyncCascadeDepth ? RequestPeerAssessment(id, asking.Append(Account.Id).ToArray())
            : null;

        private IHolder RequestPeerAssessment(AgentId id, params AgentId[] asking)
        {
            var money = RequestPeerAssessment(GetNearestFriends(id, asking).ToArray(), id, asking);
            return money.HasValue
                ? new Holder { Id = id, Money = money.Value }
                : null;
        }

        private IEnumerable<IPeer> GetNearestFriends(AgentId id, params AgentId[] asking)
            => Friends.Where(f => !asking.Contains(f.Id)).OrderBy(f => f.Id - id).Take(SyncCascadeBreadth);

        private Money? RequestPeerAssessment(IPeer[] asked, AgentId about, params AgentId[] asking)
        {
            if (!asked.Any())
                return null;
            var assessments = GetIndirectPeerAssessments(asked, about, asking);
            return assessments.Any()
                ? ComputeMeanAssessment(assessments)
                : (Money?)null;
        }

        private (IPeer, Money)[] GetIndirectPeerAssessments(IPeer[] asked, AgentId about, params AgentId[] asking) 
            => asked.Select(p => (peer: p, update: RequestPeerAssessment(p, about, asking)))
            .Where(t => t.update.PeerMoney.ContainsKey(about))
                .Select(t => (t.peer, t.update.PeerMoney[about]))
            .ToArray();

        private Update RequestPeerAssessment(IPeer asked, AgentId about, params AgentId[] asking)
            => _network.RequestUpdate(asked.Id, new[] { about }, Array.Empty<ArtefactId>(), asking);

        public bool VerifyTransaction(Transaction transaction) => Verify(transaction.Transfers);

        private bool Verify(Transfer[] transfers) => transfers.All(Verify);

        private bool Verify(Transfer transfer) => VerifyMoney(transfer) && VerifyArtfacts(transfer);

        private bool VerifyMoney(Transfer transfer) 
            => transfer.Money <= Account.GetMoney(transfer.GiverId);

        private bool VerifyArtfacts(Transfer transfer)
            => transfer.Artefacts?.All(art => VerifyArtefact(transfer.GiverId, art)) ?? true;

        private bool VerifyArtefact(AgentId giver, Artefact transferedArtefact)
            => giver == transferedArtefact.OwnerId &&
            (!Account.KnowsArtefact(transferedArtefact.Id) 
            || Account.GetArtefact(transferedArtefact.Id).OwnerId == transferedArtefact.OwnerId);

        private void SyncPeer(IPeer peer)
        {
            Sync(new[] { peer.Id }, new ArtefactId[0]);
        }

        private void Sync(AgentId[] peersToUpdate, ArtefactId[] artefactsToUpdate)
        {
            var peerUpdates = GetPeerUpdates(peersToUpdate, artefactsToUpdate);
            SyncMoney(peerUpdates);
            SyncArtefacts(peerUpdates);
        }

        private IDictionary<IPeer, Update> GetPeerUpdates(AgentId[] peersToUpdate, ArtefactId[] artefactsToUpdate)
            => Friends
                .ToDictionary(peer => peer, peer => GetUpdatesFromPeer(peer, peersToUpdate, artefactsToUpdate));

        private void SyncMoney(IDictionary<IPeer, Update> peerUpdates)
        {
            var totalTrust = Account.Peers.Sum(p => p.Trust);
            var moneyAssessments = peerUpdates
                .SelectMany(x => x.Value.PeerMoney.Select(y => (target: x.Key, subject: y.Key, money: y.Value)))
                .GroupBy(iu => iu.subject)
                .ToDictionary(
                g => Account.GetPeer(g.Key),
                g => g.Select(x => (x.target, x.money)).ToArray());
            foreach (var kvp in moneyAssessments)
                SyncMoney(totalTrust, kvp.Key, kvp.Value);
        }

        private void SyncArtefacts(IDictionary<IPeer, Update> peerUpdates)
        {
            var artefactAssessments = peerUpdates
                .SelectMany(x => x.Value.Artefacts.Select(
                    y => (target: x.Key, subject: y)))
                .GroupBy(iu => iu.subject.Id)
                .ToDictionary(
                g => Account.ProduceArtefact(g.First().subject),
                g => g.Select(x => (x.target, x.subject.OwnerId)).ToArray());
            foreach (var kvp in artefactAssessments)
                SyncArtefact(kvp.Key, kvp.Value);
        }

        private IEnumerable<IPeer> Friends => Account.Peers.Where(p => p.Trust > 0);

        private void SyncMoney(float totalTrust, IPeer subject, (IPeer target, Money assessment)[] assessments)
        {
            subject.Money = ComputeMoney(totalTrust, subject, assessments);
        }

        private void SyncArtefact(Artefact subject, (IPeer target, AgentId owner)[] assessments)
        {
            var newOwnerId = ComputeOwner(subject.OwnerId, assessments);
            if (newOwnerId != subject.OwnerId)
                Account.MoveArtefact(subject, newOwnerId);
            assessments
                .Where(a => a.owner != newOwnerId)
                .Select(a => a.target)
                .Except(new[] { Account.Self})
                .ToList()
                .ForEach(p => p.DecreaseTrust(HoldCounterfeitArtefactDistrustFactor));
        }

        private Money ComputeMoney(float totalTrust, IPeer subject, (IPeer target, Money assessment)[] assessments)
        {
            var sumOfTrusts = assessments.Sum(a => a.target.Trust);
            var meanAssessment = ComputeMeanAssessment(assessments);
            var weightedMeanAssessment = (Money)new[] { (sumOfTrusts, meanAssessment), (totalTrust - sumOfTrusts, (float)subject.Money) }.WeightedAverage();
            return weightedMeanAssessment;
        }

        private AgentId ComputeOwner(AgentId currentOwner, (IPeer target, AgentId owner)[] assessments)
        {
            var sumOfTrusts = assessments.Sum(a => a.target.Trust);
            var halfTrust = sumOfTrusts / 2;
            var accumulatedTrust = 0f;
            return assessments.OrderBy(a => a.owner == currentOwner)
                .SkipWhile(a => (accumulatedTrust += a.target.Trust) <= halfTrust)
                .First().owner;
        }

        private Money ComputeMeanAssessment((IPeer target, Money money)[] assessments)
            => (Money)assessments.Select(a => ((float)a.target.Trust, (float)a.money)).WeightedMean();

        private Update GetUpdatesFromPeer(IPeer peer, AgentId[] peersToUpdate, ArtefactId[] artefactsToUpdate)
            => _network.RequestUpdate(peer.Id, peersToUpdate, artefactsToUpdate, Account.Id);

        private void OnAddedConnection(AgentId id)
        {
            SendAction(new ConnectAction(id));
        }

        private void OnRenewedKey(byte[] oldKey, byte[] newKey)
        {
            SendAction(new RenewKeyAction(oldKey, newKey));
        }

        private void OnEndorcedAgent(AgentId agentId)
        {
            SendAction(new EndorceAction(agentId));
        }

        private void OnCreatedArtefact(Artefact artefact)
        {
            SendAction(new CreateArtefactAction(artefact));
        }

        private void OnDestroyArtefact(Artefact artefact)
        {
            SendAction(new DestroyArtefactAction(artefact));
        }

        private void OnEndorcedArtefact(Artefact artefact)
        {
            SendAction(new EndorceArtefactAction(artefact));
        }

        private void OnTransactionAccepted(Transaction transaction)
        {
            SendAction(new AcceptTransationAction(transaction));
        }

        private void OnChildCreated(INewAgent child)
        {
            SendAction(new CreateChildAction(child));
        }

        private void SendAction(IAction action)
        {
            var signedAction = Account.Sign(action);
            foreach (var peer in Account.Peers)
                _network.SendAction(peer.Id, Account.Id, signedAction);
        }

        private void SendTransaction(IPeer peer, Transaction transaction)
        {
            var action = new StartTransationAction(transaction);
            var signedAction = Account.Sign(action);
            _network.SendAction(peer.Id, Account.Id, signedAction);
        }
    }
}