using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Core.Entities
{
    public class Agent : IAgent
    {
        private readonly IDictionary<string, Relation> _relations = new Dictionary<string, Relation>();

        public Agent(IAccount account)
            : this(account.Name, account.Id, account.PublicKey, account.Peers.Select(p => p.AsRelation()))
        {
        }

        public Agent(string name, string id, byte[] publicKey)
            : this(name, id, publicKey, new Relation[0])
        {
        }

        protected Agent(string name, string id, byte[] publicKey, IEnumerable<Relation> relations)
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
        public string Id { get; }
        public byte[] PublicKey { get; set; }

        public Relation AddRelation(IAgent agent)
            => _relations[agent.Name] = new Relation(agent);

        public Relation GetRelation(string name)
            => _relations.TryGetValue(name, out var peer) ? peer : default;

        public int GetDistance(IAgent agent)
        {
            var path1 = Id.Split('.').Select(int.Parse).ToArray();
            var path2 = agent.Id.Split('.').Select(int.Parse).ToArray();
            return GetDistance(path1, path2);
        }

        private int GetDistance(int[] path1, int[] path2)
            => path1.Length > path2.Length ? GetDistance(path2, path1)
            : path1.Length == 0 ? path2.Length
            : path1[0] == path2[0] ? GetDistance(path1[1..], path2[1..])
            : Math.Abs(path1[0] - path2[0]) + path1.Length + path2.Length - 2;

        public override string ToString() => Name;
    }
}