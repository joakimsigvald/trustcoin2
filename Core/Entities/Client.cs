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

        public Update RequestUpdate(AgentId[] subjectIds, ArtefactId[] artefactIds, params AgentId[] asking)
        {
            var requestedPeers = subjectIds
                .Select(id => _actor.GetPeerAssessment(id, asking))
                .Where(pa => pa != null);
            var requestedArtefacts = _actor.Account.Artefacts
                .Select(p => p.Id)
                .Intersect(artefactIds)
                .Concat(_actor.Account.Self.OwnedArtefacts.Select(art => art.Id))
                .Distinct()
                .Select(_actor.Account.GetArtefact);
            return new Update(requestedPeers, requestedArtefacts);
        }

        public bool Update(AgentId subjectId, SignedAction signedAction)
        {
            if (!_actor.Account.IsConnectedTo(subjectId))
                return false;
            var peer = _actor.Account.GetPeer(subjectId);
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
            if (peer.IsConnectedTo(action.Model))
                return;
            peer.AddRelation(_network.FindAgent(action.Model));
        }

        private void WhenEndorce(IPeer peer, EndorceAction ea)
        {
            var relation = ProduceRelation(peer, ea.Model);
            AddMoneyFromEndorcement(peer, relation);
            relation.Endorce();
        }

        private void WhenEndorceArtefact(IPeer peer, ArtefactAction action)
        {
            var artefact = action.Model;
            if (_actor.Account.KnowsArtefact(action.Model.Id))
            {
                artefact = _actor.Account.GetArtefact(artefact.Id);
                if (artefact.OwnerId != action.Model.OwnerId)
                {
                    peer.DecreaseTrust(EndorceCounterfeitArtefactDistrustFactor);
                    return;
                }
            }
            else
                _actor.Account.RememberArtefact(artefact);

            var relation = ProduceRelation(peer, artefact.OwnerId);
            AddMoneyFromEndorcement(peer, relation, ArtefactMoneyFactor);
            relation.IncreaseStrength(ArtefactEndorcementTrustFactor);
        }

        private void WhenCreateArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Model.Id))
                return;
            if (_actor.Account.KnowsArtefact(action.Model.Id))
                peer.DecreaseTrust(MakeCounterfeitArtefactDistrustFactor);
            else
                _actor.Account.AddArtefact(action.Model, peer.Id);
        }

        private void WhenDestroyArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Model.Id))
                _actor.Account.ForgetArtefact(action.Model.Id);
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
            if (_actor.VerifyTransaction(action.Model))
                AccountTransaction(action);
            else
                RejectTransaction(action);
        }

        private void WhenCreateChild(CreateChildAction action)
        {
            _actor.Connect(action.Model.Id);
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
                AccountArtefactTransfer(transfer.Artefacts, transfer.ReceiverId);
            if (transfer.Money > 0f)
                AccountMoneyTransfer(transfer.Money, transfer.GiverId, transfer.ReceiverId);
            _actor.Account.IncreaseTrust(transfer.ReceiverId, AccountedTransactionTrustFactor);
            _actor.Account.IncreaseTrust(transfer.GiverId, AccountedTransactionTrustFactor);
        }

        private void AccountArtefactTransfer(Artefact[] artefacts, AgentId receiverId)
        {
            foreach (var artefact in artefacts)
                _actor.Account.MoveArtefact(
                    artefact,
                    _actor.ProducePeer(receiverId).Id);
        }

        private void AccountMoneyTransfer(Money money, AgentId giverId, AgentId receiverId)
        {
            _actor.Account.DecreaseMoney(giverId, money);
            _actor.Account.IncreaseMoney(receiverId, money);
        }

        private void RejectTransaction(AcceptTransationAction action)
        {
            var involvedParties = action.Model.Transfers
                .SelectMany(x => new[] { x.GiverId, x.ReceiverId })
                .Distinct()
                .ToList();
            involvedParties.ForEach(ip => _actor.Account.DecreaseTrust(ip, UnaccountedTransactionDistrustFactor));
        }

        private void AddMoneyFromEndorcement(IPeer endorcer, Relation relation, float factor = 1)
        {
            if (!_actor.Account.IsConnectedTo(relation.Agent.Id))
                return;
            var addedMoney = factor * endorcer.Trust * (1 - (float)relation.Strength);
            if (addedMoney <= 0) return;
            var endorcedPeer = _actor.Account.GetPeer(relation.Agent.Id);
            endorcedPeer.Money += (Money)addedMoney;
        }

        private Relation ProduceRelation(IPeer source, AgentId targetId)
            => source.GetRelation(targetId)
                ?? source.AddRelation(_network.FindAgent(targetId));
    }
}