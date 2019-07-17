using System;
using System.Linq;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Client : IClient
    {
        private readonly IActor _actor;
        private readonly INetwork _network;

        public Client(INetwork network, IActor actor)
        {
            _network = network;
            _actor = actor;
        }

        public Update RequestUpdate(string[] subjectNames, string[] artefactNames)
        {
            var requestedPeers = _actor.Account.Peers.Select(p => p.Name)
                .Intersect(subjectNames)
                .Select(_actor.Account.GetPeer);
            var requestedArtefacts = _actor.Account.Artefacts.Select(p => p.Name)
                .Intersect(artefactNames)
                .Select(_actor.Account.GetArtefact);
            return new Update(requestedPeers, requestedArtefacts);
        }

        public bool Update(string subjectName, SignedAction signedAction)
        {
            if (!_actor.Account.IsConnectedTo(subjectName))
                return false;
            var peer = _actor.Account.GetPeer(subjectName);
            _actor.Account.VerifySignature(signedAction, peer);
            UpdatePeer(peer, signedAction.Action);
            return true;
        }

        private void UpdatePeer(IPeer peer, IAction action)
        {
            switch (action)
            {
                case ConnectAction ca:
                    WhenConnect(peer, ca);
                    break;
                case EndorceAction ea:
                    WhenEndorce(peer, ea);
                    break;
                case RenewKeyAction ra:
                    WhenRenewKey(peer, ra);
                    break;
                case CreateArtefactAction caa:
                    WhenCreateArtefact(peer, caa);
                    break;
                case DestroyArtefactAction daa:
                    WhenDestroyArtefact(peer, daa);
                    break;
                case EndorceArtefactAction daa:
                    WhenEndorceArtefact(peer, daa);
                    break;
                case StartTransationAction sta:
                    WhenStartTransaction(sta);
                    break;
                case AcceptTransationAction ata:
                    WhenAcceptTransaction(ata);
                    break;
                case CreateChildAction cca:
                    WhenCreateChild(cca);
                    break;
                case NoAction _:
                    break;
                default: throw new NotImplementedException("Action not implemented: " + action);
            }
        }

        private void WhenRenewKey(IPeer peer, RenewKeyAction ra)
        {
            if (peer.PublicKey.SequenceEqual(ra.NewKey))
                return;
            if (!peer.PublicKey.SequenceEqual(ra.OldKey))
                throw new InvalidOperationException($"Invalid old key: {ra.OldKey}, expected: {peer.PublicKey}");
            peer.PublicKey = ra.NewKey;
        }

        private void WhenConnect(IPeer peer, ConnectAction action)
        {
            if (peer.IsConnectedTo(action.AgentName))
                return;
            peer.AddRelation(_network.FindAgent(action.AgentName));
        }

        private void WhenEndorce(IPeer peer, EndorceAction ea)
        {
            var relation = ProduceRelation(peer, ea.AgentName);
            AddMoneyFromEndorcement(peer, relation);
            relation.Endorce();
        }

        private void WhenEndorceArtefact(IPeer peer, ArtefactAction action)
        {
            var artefact = action.Model;
            if (_actor.Account.KnowsArtefact(action.Model.Name))
            {
                artefact = _actor.Account.GetArtefact(artefact.Name);
                if (artefact.OwnerName != action.Model.OwnerName)
                {
                    peer.DecreaseTrust(EndorceCounterfeitArtefactDistrustFactor);
                    return;
                }
            }
            else
                _actor.Account.RememberArtefact(artefact);

            var relation = ProduceRelation(peer, artefact.OwnerName);
            AddMoneyFromEndorcement(peer, relation, ArtefactMoneyFactor);
            relation.IncreaseStrength(ArtefactEndorcementTrustFactor);
        }

        private void WhenCreateArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Model.Name))
                return;
            if (_actor.Account.KnowsArtefact(action.Model.Name))
                peer.DecreaseTrust(MakeCounterfeitArtefactDistrustFactor);
            else
                _actor.Account.AddArtefact(action.Model.Name, peer.Name);
        }

        private void WhenDestroyArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Model.Name))
                _actor.Account.RemoveArtefact(action.Model);
            else
                peer.DecreaseTrust(DestroyOthersArtefactDistrustFactor);
        }

        private void WhenStartTransaction(StartTransationAction action)
        {
            if (_actor.Account.HasReceivedTransaction(action.Model.Key))
                return;
            _actor.Account.AddTransaction(action.Model);
        }

        private void WhenAcceptTransaction(AcceptTransationAction action)
        {
            if (_actor.Account.HasReceivedTransaction(action.Model.Key))
                return;
            _actor.Account.AddReceivedTransaction(action.Model.Key);
            if (Verify(action.Model) ?? true)
                AccountTransaction(action);
            else
                RejectTransaction(action);
        }

        private void WhenCreateChild(CreateChildAction action)
        {
            _actor.Connect(action.Model.Name);
        }

        private void AccountTransaction(AcceptTransationAction action)
        {
            foreach (var transfer in action.Model.Transfers)
                AccountTransfer(transfer);
            if (_actor.Account.HasPendingTransaction(action.Model.Key))
                _actor.RelayTransactionAccepted(action.Model);
        }

        private void AccountTransfer(Transfer transfer)
        {
            if (transfer.Artefacts != null)
                AccountArtefactTransfer(transfer.Artefacts, transfer.ReceiverName);
            if (transfer.Money > 0f)
                AccountMoneyTransfer(transfer.Money, transfer.GiverName, transfer.ReceiverName);
            _actor.Account.IncreaseTrust(transfer.ReceiverName, AccountedTransactionTrustFactor);
            _actor.Account.IncreaseTrust(transfer.GiverName, AccountedTransactionTrustFactor);
        }

        private void AccountArtefactTransfer(Artefact[] artefacts, string receiverName)
        {
            foreach (var artefact in artefacts)
                _actor.Account.MoveArtefact(
                    artefact,
                    _actor.ProducePeer(receiverName).Name);
        }

        private void AccountMoneyTransfer(Money money, string giverName, string receiverName)
        {
            _actor.Account.DecreaseMoney(giverName, money);
            _actor.Account.IncreaseMoney(receiverName, money);
        }

        private void RejectTransaction(AcceptTransationAction action)
        {
            var involvedParties = action.Model.Transfers
                .SelectMany(x => new[] { x.GiverName, x.ReceiverName })
                .Distinct()
                .ToList();
            involvedParties.ForEach(ip => _actor.Account.DecreaseTrust(ip, UnaccountedTransactionDistrustFactor));
        }

        private void AddMoneyFromEndorcement(IPeer endorcer, Relation relation, float factor = 1)
        {
            if (!_actor.Account.IsConnectedTo(relation.Agent.Name))
                return;
            var addedMoney = factor * endorcer.Trust * (1 - (float)relation.Strength);
            if (addedMoney <= 0) return;
            var endorcedPeer = _actor.Account.GetPeer(relation.Agent.Name);
            endorcedPeer.Money += (Money)addedMoney;
        }

        private Relation ProduceRelation(IPeer source, string targetName)
            => source.GetRelation(targetName)
                ?? source.AddRelation(_network.FindAgent(targetName));

        public bool? Verify(Transaction transaction)
            => AggregateVerifications(transaction.Transfers.Select(Verify).ToArray());

        private bool? Verify(Transfer transfer)
            => AggregateVerifications(Verify(transfer.Artefacts, transfer.GiverName), Verify(transfer.Money, transfer.GiverName));

        private bool? Verify(Artefact[] artefacts, string giverName)
            => artefacts is null ? true
            : artefacts.Any(a => a.OwnerName != giverName) ? false
            : AggregateVerifications(artefacts.Select(Verify).ToArray());

        private bool? Verify(Artefact artefact)
            => !_actor.Account.KnowsArtefact(artefact.Name)
                       ? (bool?)null
                       : _actor.Account.GetArtefact(artefact.Name).OwnerName == artefact.OwnerName;

        private bool? Verify(Money money, string giverName)
            => money == 0f ? true
            : _actor.Account.IsConnectedTo(giverName)
            ? _actor.Account.GetMoney(giverName) >= money
            : (bool?)null;

        private bool? AggregateVerifications(params bool?[] verifications)
            => verifications.Any(v => v == false)
            ? false
            : verifications.Any(v => v is null)
            ? (bool?)null
            : true;
    }
}