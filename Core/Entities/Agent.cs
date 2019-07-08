using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Core.Entities
{
    public class Agent : IAgent
    {
        private readonly IDictionary<string, Relation> _relations = new Dictionary<string, Relation>();

        protected Agent(string name, byte[] publicKey, IEnumerable<Relation> relations)
        {
            Name = name;
            PublicKey = publicKey;
            _relations = relations.ToDictionary(r => r.Agent.Name);
        }

        public Agent(IAccount account)
            : this(account.Name, account.PublicKey, account.Peers.Select(p => p.AsRelation()))
        {
        }


        public IPeer AsPeer()
            => new Peer(Name, PublicKey, Relations);

        public IAgent Clone()
            => new Agent(Name, PublicKey, Relations);

        public ICollection<Relation> Relations => _relations.Values;
        public bool IsConnectedTo(string name) => _relations.ContainsKey(name);

        public string Name { get; private set; }
        public byte[] PublicKey { get; set; }

        public Relation AddRelation(IAgent agent)
            => _relations[agent.Name] = new Relation(agent);

        public Relation GetRelation(string name)
            => _relations.TryGetValue(name, out var peer) ? peer : default;

        public override string ToString() => Name;
    }
}