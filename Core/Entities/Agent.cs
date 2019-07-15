using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public class Agent : IAgent
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
        {
            Name = name;
            Id = id;
            PublicKey = publicKey;
            _relations = relations.ToDictionary(r => r.Agent.Name);
        }

        public IPeer AsPeer()
            => new Peer(Name, Id, PublicKey, Relations);

        public IAgent Clone()
            => new Agent(Name, Id, PublicKey, Relations);

        public ICollection<Relation> Relations => _relations.Values;
        public bool IsConnectedTo(string name) => _relations.ContainsKey(name);

        public string Name { get; private set; }
        public AgentId Id { get; }
        public byte[] PublicKey { get; set; }

        public Relation AddRelation(IAgent agent)
            => _relations[agent.Name] = new Relation(agent);

        public Relation GetRelation(string name)
            => _relations.TryGetValue(name, out var peer) ? peer : default;

        public override string ToString() => Name;
    }
}