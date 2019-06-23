using System.Collections.Generic;

namespace Trustcoin.Business
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

        public IPeer Me => Peer.GetSelf(this);

        public IPeer Connect(string name) 
            => _peers[name] = Peer.GetPeer(this, _network.FindAgent(name));

        public bool IsConnected(string name)
            => _peers.ContainsKey(name);

        public ICollection<IPeer> Peers => _peers.Values;

        public void Endorce(string name)
        {
            (IsConnected(name) ? GetPeer(name) : Connect(name)).Endorce(this);
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
    }
}