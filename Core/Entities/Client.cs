using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Client : IClient
    {
        private readonly IAccount _account;
        private readonly INetwork _network;

        public Client(INetwork network, IAccount account)
        {
            _network = network;
            _account = account;
        }

        public IDictionary<string, Money> RequestUpdate(string[] subjectNames)
        {
            var requestedPeers = _account.Peers.Select(p => p.Name)
                .Intersect(subjectNames)
            .Select(_account.GetPeer)
            .ToArray();
            return requestedPeers.ToDictionary(p => p.Name, p => p.Money);
        }

        public bool Update(string subjectName, SignedAction signedAction)
        {
            if (!_account.IsConnectedTo(subjectName))
                return false;
            var peer = _account.GetPeer(subjectName);
            _account.VerifySignature(signedAction, peer);
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
            if (_account.KnowsArtefact(action.Artefact.Name))
            {
                artefact = _account.GetArtefact(artefact.Name);
                if (artefact.OwnerName != action.Artefact.OwnerName)
                {
                    peer.DecreaseTrust(EndorceCounterfeitArtefactDistrustFactor);
                    return;
                }
            }
            else
                _account.RememberArtefact(artefact);

            var relation = ProduceRelation(peer, artefact.OwnerName);
            AddMoneyFromEndorcement(peer, relation, ArtefactMoneyFactor);
            relation.IncreaseStrength(ArtefactEndorcementTrustFactor);
        }

        private void WhenCreateArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Artefact.Name))
                return;
            if (_account.KnowsArtefact(action.Artefact.Name))
                peer.DecreaseTrust(CounterfeitArtefactDistrustFactor);
            else
                AddArtefact(peer, action.Artefact);
        }

        private void WhenDestroyArtefact(IPeer peer, ArtefactAction action)
        {
            if (peer.HasArtefact(action.Artefact.Name))
                DestroyArtefact(peer, action.Artefact);
            else
                peer.DecreaseTrust(DestroyOthersArtefactDistrustFactor);
        }

        private void AddMoneyFromEndorcement(IPeer endorcer, Relation relation, float factor = 1)
        {
            if (!_account.IsConnectedTo(relation.Agent.Name))
                return;
            var addedMoney = factor * endorcer.Trust * (1 - (float)relation.Strength);
            if (addedMoney <= 0) return;
            var endorcedPeer = _account.GetPeer(relation.Agent.Name);
            endorcedPeer.Money += (Money)addedMoney;
        }

        public void AddArtefact(IPeer peer, IArtefact artefact)
        {
            peer.AddArtefact(artefact);
            _account.RememberArtefact(artefact);
        }

        public void DestroyArtefact(IPeer peer, IArtefact artefact)
        {
            peer.RemoveArtefact(artefact);
            _account.ForgetArtefact(artefact.Name);
        }

        private Relation ProduceRelation(IPeer source, string targetName)
            => source.GetRelation(targetName)
                ?? source.AddRelation(_network.FindAgent(targetName));
    }
}