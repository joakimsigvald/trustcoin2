using System;
using System.Collections.Generic;
using Trustcoin.Core.Actions;

namespace Trustcoin.Core
{
    public class Account : IAccount, IClient
    {
        private readonly IDictionary<string, IPeer> _peers = new Dictionary<string, IPeer>();
        private readonly INetwork _network;

        public Account(INetwork network, string name)
        {
            _network = network;
            Name = name;
            RenewKeys();
        }

        public string Name { get; private set; }
        public string PublicKey { get; private set; }

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
            (IsConnectedTo(name) ? GetPeer(name) : Connect(name)).Endorce();
            OnEndorcedAgent(name);
        }

        public IPeer GetPeer(string name)
            => _peers.TryGetValue(name, out var peer)
            ? peer
            : throw new NotFound<Peer>(name);

        public Weight GetTrust(string name)
            => GetPeer(name).Trust;

        public Weight SetTrust(string name, Weight trust) => GetPeer(name).Trust = trust;

        public Weight IncreaseTrust(string name, Weight factor) => GetPeer(name).IncreaseTrust(factor);

        public Weight ReduceTrust(string name, Weight factor) => GetPeer(name).ReduceTrust(factor);

        public override string ToString() => Name;

        public bool Update(string sourceAgentName, IAction action)
        {
            if (!IsConnectedTo(sourceAgentName))
                return false;
            var peer = GetPeer(sourceAgentName);
            if (!action.SourceSignature.Verify(sourceAgentName, peer.PublicKey))
                throw new InvalidOperationException("Source Signature is not valid");
            UpdatePeer(peer, action);
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
            relation.IsEndorced = true;
        }

        private void WhenConnect(IPeer peer, ConnectAction action)
        {
            if (peer.IsConnectedTo(action.AgentName))
                return;
            peer.AddRelation(_network.FindAgent(action.AgentName));
        }

        public void RenewKeys()
        {
            var oldKey = PublicKey;
            PublicKey = GenerateKey();
            OnRenewedKey(oldKey, PublicKey);
        }

        private string GenerateKey()
            => $"{DateTime.UtcNow.Ticks}";

        private void OnAddedConnection(string agentName)
        {
            SendAction(new ConnectAction(Sign(Name), agentName));
        }

        private void OnRenewedKey(string oldKey, string newKey)
        {
            SendAction(new RenewKeyAction(Sign(Name), oldKey, newKey));
        }

        private void OnEndorcedAgent(string agentName)
        {
            SendAction(new EndorceAction(Sign(Name), agentName));
        }

        private void SendAction(IAction action)
        {
            foreach (var peer in Peers)
                _network.SendAction(peer.Name, Name, action);
        }

        private ISignature Sign(string name)
            => new FakeSignature(name);
    }
}