using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public class Agent : NewAgent, IAgent
    {
        private readonly IDictionary<string, Relation> _relations = new Dictionary<string, Relation>();

        public Agent(IAccount account)
            : this(account.Name, account.Id, account.PublicKey, account.Peers.Select(p => p.AsRelation()))
        {
        }

        public Agent(string name, AgentId id, byte[] publicKey)
            : this(name, id, publicKey, new Relation[0])
        {
        }

        protected Agent(string name, AgentId id, byte[] publicKey, IEnumerable<Relation> relations)
            : base(name, id, publicKey)
        {
            _relations = relations.ToDictionary(r => r.Agent.Name);
        }

        public IPeer AsPeer()
            => new Peer(Name, Id, PublicKey, Relations);

        public override IAgent Clone()
            => new Agent(Name, Id, PublicKey, Relations);

        public ICollection<Relation> Relations => _relations.Values;
        public bool IsConnectedTo(string name) => _relations.ContainsKey(name);

        public Relation AddRelation(IAgent agent)
            => _relations[agent.Name] = new Relation(agent);

        public Relation GetRelation(string name)
            => _relations.TryGetValue(name, out var peer) ? peer : default;
    }
}