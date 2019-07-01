using System;
using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public class Account : IAccount, IClient
    {
        private readonly IDictionary<string, IPeer> _peers = new Dictionary<string, IPeer>();
        private readonly INetwork _network;
        private readonly ICryptography _cryptography;

        public Account(INetwork network, ICryptography cryptography, string name)
        {
            _network = network;
            _cryptography = cryptography;
            Name = name;
        }

        public string Name { get; private set; }
        public byte[] PublicKey => _cryptography.PublicKey;

        public IPeer Self => Peer.GetSelf(this);

        public IPeer Connect(string name)
        {
            if (name == Name)
                throw new InvalidOperationException("Cannot connect with self");
            var newPeer = Peer.MakePeer(_network.FindAgent(name));
            _peers[name] = newPeer;
            OnAddedConnection(name);
            return newPeer;
        }

        public bool IsConnectedTo(string name)
            => _peers.ContainsKey(name);

        public ICollection<IPeer> Peers => _peers.Values;

        public void Endorce(string name)
        {
            ProducePeer(name).Endorce();
            OnEndorcedAgent(name);
        }

        public IPeer GetPeer(string name)
            => _peers.TryGetValue(name, out var peer)
            ? peer
            : throw new NotFound<Peer>(name);

        public Weight GetTrust(string name) => GetPeer(name).Trust;
        public Weight SetTrust(string name, Weight trust) => GetPeer(name).Trust = trust;
        public Weight IncreaseTrust(string name, Weight factor) => GetPeer(name).IncreaseTrust(factor);
        public Weight ReduceTrust(string name, Weight factor) => GetPeer(name).ReduceTrust(factor);

        public Money GetMoney(string name) => GetPeer(name).Money;
        public void SetMoney(string name, Money money) => GetPeer(name).Money = money;

        public override string ToString() => Name;

        public bool Update(string sourceAgentName, ISignedAction signedAction)
        {
            if (!IsConnectedTo(sourceAgentName))
                return false;
            var peer = GetPeer(sourceAgentName);
            _cryptography.VerifySignature(signedAction, peer);
            UpdatePeer(peer, signedAction.Action);
            return true;
        }

        private void UpdatePeer(IPeer peer, IAction action) {
            switch (action) {
                case ConnectAction ca:
                    WhenConnect(peer, ca);
                    break;
                case EndorceAction ea:
                    WhenEndorce(peer, ea);
                    break;
                case RenewKeyAction ra:
                    WhenRenewKey(peer, ra);
                    break;
                case NoAction _:
                    break;
                default: throw new NotImplementedException("Action not implemented: " + action);
            }
        }

        private void WhenRenewKey(IPeer peer, RenewKeyAction ra)
        {
            if (peer.PublicKey == ra.NewKey)
                return;
            if (peer.PublicKey != ra.OldKey)
                throw new InvalidOperationException($"Invalid old key: {ra.OldKey}, expected: {peer.PublicKey}");
            peer.PublicKey = ra.NewKey;
        }

        private void WhenEndorce(IPeer peer, EndorceAction ea)
        {
            if (peer.Endorces(ea.AgentName))
                return;
            var relation = peer.GetRelation(ea.AgentName);
            if (!relation.IsConnected)
                relation = peer.AddRelation(_network.FindAgent(ea.AgentName));
            AddMoneyFromEndorcement(peer, relation);
            relation.IsEndorced = true;
        }

        private void AddMoneyFromEndorcement(IPeer endorcer, Relation relation)
        {
            var addedMoney = (2 * endorcer.Trust - 1) * (1 - (float)relation.Weight);
            if (addedMoney <= 0) return;
            var endorcedPeer = ProducePeer(relation.Agent.Name);
            endorcedPeer.Money += (Money)addedMoney;
        }

        private IPeer ProducePeer(string name)
            => IsConnectedTo(name) ? GetPeer(name) : Connect(name);

        private void WhenConnect(IPeer peer, ConnectAction action)
        {
            if (peer.IsConnectedTo(action.AgentName))
                return;
            peer.AddRelation(_network.FindAgent(action.AgentName));
        }

        public void RenewKeys()
        {
            var oldKey = PublicKey;
            _cryptography.RenewKeys();
            OnRenewedKey(oldKey, PublicKey);
        }

        public void SetRelationWeight(string subjectName, string objectName, Weight value)
        {
            var subject = GetPeer(subjectName);
            var relation = subject.GetRelation(objectName);
            relation.Weight = value;
        }

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

        private void SendAction(IAction action)
        {
            var signedAction = _cryptography.Sign(action);
            foreach (var peer in Peers)
                _network.SendAction(peer.Name, Name, signedAction);
        }
    }
}