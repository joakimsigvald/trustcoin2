using System;
using System.Collections.Generic;
using Trustcoin.Core;

namespace Trustcoin.Core
{
    public class Account : IAccount
    {
        private readonly IDictionary<string, IPeer> _peers = new Dictionary<string, IPeer>();
        private readonly INetwork _network;

        public Account(INetwork network, string name)
        {
            _network = network;
            Name = name;
        }

        public string Name { get; private set; }

        public IPeer Self => Peer.GetSelf(this);

        public IPeer Connect(string name)
        {
            var newPeer = Peer.GetPeer(this, _network.FindAgent(name));
            _peers[name] = newPeer;
            UpdatePeers();
            return newPeer;
        }

        public bool IsConnectedTo(string name)
            => _peers.ContainsKey(name);

        public ICollection<IPeer> Peers => _peers.Values;

        public void Endorce(string name)
        {
            (IsConnectedTo(name) ? GetPeer(name) : Connect(name)).Endorce(this);
            UpdatePeers();
        }

        public IPeer GetPeer(string name)
            => _peers.TryGetValue(name, out var peer)
            ? peer
            : throw new NotFound<Peer>(name);

        public float GetTrust(string name)
            => GetPeer(name).Trust;

        public float SetTrust(string name, float trust) => GetPeer(name).Trust = trust;

        public float IncreaseTrust(string name, float factor) => GetPeer(name).IncreaseTrust(factor);

        public float ReduceTrust(string name, float factor) => GetPeer(name).ReduceTrust(factor);

        public bool Update(IAgent sourceAgent, ISignature sourceSignature)
        {
            if (!IsConnectedTo(sourceAgent.Name))
                throw new InvalidOperationException("I am not connected to source");
            if (!sourceSignature.Verify(sourceAgent.Name))
                throw new InvalidOperationException("Source Signature is not valid");
            var peer = GetPeer(sourceAgent.Name);
            peer.UpdateRelations(sourceAgent);
            return true;
        }

        private void UpdatePeers()
        {
            foreach (var peer in Peers)
                _network.Update(peer.Name, Name, Sign(Name));
        }

        private ISignature Sign(string name)
            => new FakeSignature(name);
    }
}