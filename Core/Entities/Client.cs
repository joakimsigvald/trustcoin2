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
            var artefact = action.Artefact;
            if (_actor.Account.KnowsArtefact(action.Artefact.Name))
            {
                artefact = _actor.Account.GetArtefact(artefact.Name);
                if (artefact.OwnerName != action.Artefact.OwnerName)
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
            if (peer.HasArtefact(action.Artefact.Name))
                return;
            if (_actor.Account.KnowsArtefact(action.Artefact.Name))
                peer.DecreaseTrust(MakeCounterfeitArtefactDistrustFactor);
            else
                _actor.Account.AddArtefact(action.Artefact.Name, peer.Name);
        }

        private void WhenDestroyArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Artefact.Name))
                _actor.Account.RemoveArtefact(action.Artefact);
            else
                peer.DecreaseTrust(DestroyOthersArtefactDistrustFactor);
        }

        private void WhenStartTransaction(StartTransationAction action)
        {
            if (_actor.Account.HasReceivedTransaction(action.Transaction.Key))
                return;
            _actor.Account.AddTransaction(action.Transaction);
        }

        private void WhenAcceptTransaction(AcceptTransationAction action)
        {
            if (_actor.Account.HasReceivedTransaction(action.Transaction.Key))
                return;
            _actor.Account.AddReceivedTransaction(action.Transaction.Key);
            if (Verify(action.Transaction) ?? true)
                AccountTransaction(action);
            else
                RejectTransaction(action);
        }

        private void AccountTransaction(AcceptTransationAction action)
        {
            _actor.Account.MoveArtefact(
                action.Transaction.Artefact,
                _actor.ProducePeer(action.Transaction.ReceiverName).Name);
            _actor.Account.IncreaseTrust(action.Transaction.ReceiverName, AccountedTransactionTrustFactor);
            _actor.Account.IncreaseTrust(action.Transaction.Artefact.OwnerName, AccountedTransactionTrustFactor);
            if (_actor.Account.HasPendingTransaction(action.Transaction.Key))
                _actor.RelayTransactionAccepted(action.Transaction);
        }

        private void RejectTransaction(AcceptTransationAction action)
        {
            _actor.Account.DecreaseTrust(action.Transaction.ReceiverName, UnaccountedTransactionDistrustFactor);
            _actor.Account.DecreaseTrust(action.Transaction.Artefact.OwnerName, UnaccountedTransactionDistrustFactor);
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
            => !_actor.Account.KnowsArtefact(transaction.Artefact.Name)
            ? (bool?)null
            : _actor.Account.GetArtefact(transaction.Artefact.Name).OwnerName == transaction.Artefact.OwnerName;
    }
}