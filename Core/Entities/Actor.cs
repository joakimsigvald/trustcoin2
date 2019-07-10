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
        public IAccount Account { get; }

        public Actor(INetwork network, IAccount account)
        {
            _network = network;
            Account = account;
        }

        public string Name => Account.Name;

        public IPeer ProducePeer(string name)
            => Account.IsConnectedTo(name) ? Account.GetPeer(name) : Connect(name);

        public IPeer Connect(string name)
        {
            if (Account.IsConnectedTo(name))
                return Account.GetPeer(name);
            var newPeer = _network.FindAgent(name).AsPeer();
            Account.AddPeer(newPeer);
            SyncPeer(newPeer);
            OnAddedConnection(name);
            return newPeer;
        }

        public void Endorce(string name)
        {
            ProducePeer(name).Endorce();
            OnEndorcedAgent(name);
        }

        public void EndorceArtefact(IArtefact artefact)
        {
            if (artefact.OwnerName is null)
                throw new ArgumentException("Tried to endorce artefact without owner");
            var owner = ProducePeer(artefact.OwnerName);
            if (Account.KnowsArtefact(artefact.Name))
            {
                artefact = Account.GetArtefact(artefact.Name);
                if (owner.Name != artefact.OwnerName)
                    throw new ArgumentException($"Tried to endorce artefact with invalid owner: {owner.Name}");
            }
            else
                Account.RememberArtefact(artefact);
            Account.IncreaseTrust(owner.Name, ArtefactEndorcementTrustFactor);
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
                Account.Peers.Select(peer => peer.Name).ToArray(), 
                Account.Artefacts.Select(a => a.Name).ToArray());
        }

        public IArtefact CreateArtefact(string name)
        {
            var artefact = new Artefact(name, Account.Name);
            OnCreatedArtefact(artefact);
            return artefact;
        }

        public void DestroyArtefact(string name)
        {
            if (!Account.KnowsArtefact(name) || Account.GetArtefact(name).OwnerName != Account.Name)
                throw new ArgumentException("Tried to destroy artefact not owned");
            OnDestroyArtefact(Account.GetArtefact(name));
        }

        private void SyncPeer(IPeer peer)
        {
            Sync(new[] { peer.Name }, new string[0]);
        }

        private void Sync(string[] peersToUpdate, string[] artefactsToUpdate)
        {
            var peerUpdates = GetPeerUpdates(peersToUpdate, artefactsToUpdate);
            SyncMoney(peerUpdates);
            SyncArtefacts(peerUpdates);
        }

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
                .SelectMany(x => x.Value.ArtefactOwners.Select(
                    y => (target: x.Key, subject: y.Key, owner: y.Value)))
                .GroupBy(iu => iu.subject)
                .ToDictionary(
                g => Account.GetArtefact(g.Key),
                g => g.Select(x => (x.target, x.owner)).ToArray());
            foreach (var kvp in artefactAssessments)
                SyncArtefact(kvp.Key, kvp.Value);
        }

        private IDictionary<IPeer, Update> GetPeerUpdates(string[] peersToUpdate, string[] artefactsToUpdate)
            => TrustedPeers
                .ToDictionary(peer => peer, peer => GetUpdatesFromPeer(peer, peersToUpdate, artefactsToUpdate));

        private IEnumerable<IPeer> TrustedPeers => Account.Peers.Where(p => p.Trust > 0);

        private void SyncMoney(float totalTrust, IPeer subject, (IPeer target, Money assessment)[] assessments)
        {
            subject.Money = ComputeMoney(totalTrust, subject, assessments);
        }

        private void SyncArtefact(IArtefact subject, (IPeer target, string owner)[] assessments)
        {
            var newOwnerName = ComputeOwner(subject, assessments);
            if (newOwnerName != subject.OwnerName)
                MoveArtefact(subject, newOwnerName);
            assessments
                .Where(a => a.owner != newOwnerName)
                .Select(a => a.target)
                .Except(new[] { Account.Self})
                .ToList()
                .ForEach(p => p.DecreaseTrust(HoldCounterfeitArtefactDistrustFactor));
        }

        private void MoveArtefact(IArtefact artefact, string toPeerName)
        {
            Account.RemoveArtefact(artefact);
            Account.AddArtefact(artefact.Name, toPeerName);
        }

        private Money ComputeMoney(float totalTrust, IPeer subject, (IPeer target, Money assessment)[] assessments)
        {
            var sumOfTrusts = assessments.Sum(a => a.target.Trust);
            var meanAssessment = ComputeMeanAssessment(assessments);
            var weightedMeanAssessment = (Money)new[] { (sumOfTrusts, meanAssessment), (totalTrust - sumOfTrusts, (float)subject.Money) }.WeightedAverage();
            return weightedMeanAssessment;
        }

        private string ComputeOwner(IArtefact subject, (IPeer target, string owner)[] assessments)
        {
            var sumOfTrusts = assessments.Sum(a => a.target.Trust);
            var halfTrust = sumOfTrusts / 2;
            var accumulatedTrust = 0f;
            return assessments.OrderBy(a => a.owner == subject.OwnerName)
                .SkipWhile(a => (accumulatedTrust += a.target.Trust) <= halfTrust)
                .First().owner;
        }

        private Money ComputeMeanAssessment((IPeer target, Money money)[] assessments)
            => (Money)assessments.Select(a => ((float)a.target.Trust, (float)a.money)).WeightedMean();

        private Update GetUpdatesFromPeer(IPeer peer, string[] peersToUpdate, string[] artefactsToUpdate)
            => _network.RequestUpdate(
                peer.Name,
                peer.Relations.Select(r => r.Agent.Name).Intersect(peersToUpdate).ToArray(),
                artefactsToUpdate);

        private void OnAddedConnection(string agentName)
        {
            SendAction(new ConnectAction(agentName));
        }

        private void OnRenewedKey(byte[] oldKey, byte[] newKey)
        {
            SendAction(new RenewKeyAction(oldKey, newKey));
        }

        private void OnEndorcedAgent(string agentName)
        {
            SendAction(new EndorceAction(agentName));
        }

        private void OnCreatedArtefact(IArtefact artefact)
        {
            SendAction(new CreateArtefactAction(artefact));
        }

        private void OnDestroyArtefact(IArtefact artefact)
        {
            SendAction(new DestroyArtefactAction(artefact));
        }

        private void OnEndorcedArtefact(IArtefact artefact)
        {
            SendAction(new EndorceArtefactAction(artefact));
        }

        private void SendAction(IAction action)
        {
            var signedAction = Account.Sign(action);
            foreach (var peer in Account.Peers)
                _network.SendAction(peer.Name, Account.Name, signedAction);
        }
    }
}