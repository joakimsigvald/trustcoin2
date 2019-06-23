using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Business
{
    public class Agent : IAgent
    {
        public bool IsEndorced { get; private set; }
        private readonly IDictionary<string, IAgent> _relations = new Dictionary<string, IAgent>();

        public Agent(IAccount account)
        {
            Name = account.Name;
            _relations = account.Peers.ToDictionary(p => p.Name, p => (IAgent)new Agent(p));
        }

        public Agent(IPeer peer)
        {
            Name = peer.Name;
            IsEndorced = peer.IsEndorced;
            _relations = peer.Relations.ToDictionary(r => r.Name);
        }

        public ICollection<IAgent> Relations => _relations.Values;

        public string Name { get; private set; }

        public bool Endorces(string name) => GetRelation(name)?.IsEndorced ?? false;

        private IAgent GetRelation(string name)
            => _relations.TryGetValue(name, out var peer) ? peer : null;
    }
}