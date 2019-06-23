using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Core
{
    public class Agent : IAgent
    {
        public bool IsEndorced { get; protected set; }
        private IDictionary<string, IAgent> _relations = new Dictionary<string, IAgent>();

        protected Agent(string name, string publicKey, IEnumerable<IAgent> relations)
        {
            Name = name;
            PublicKey = publicKey;
            _relations = relations.ToDictionary(r => r.Name);
        }

        public Agent(IAccount account)
            : this(account.Name, account.PublicKey, account.Peers.Select(p => p.Clone()))
        {
        }

        public IAgent Clone()
            => new Agent(Name, PublicKey, Relations)
            {
                IsEndorced = IsEndorced
            };

        public ICollection<IAgent> Relations => _relations.Values;
        public bool IsConnectedTo(string name) => _relations.ContainsKey(name);

        public string Name { get; private set; }
        public string PublicKey { get; private set; }

        public bool Endorces(string name) => GetRelation(name)?.IsEndorced ?? false;

        public void Update(IAgent sourceAgent)
        {
            _relations = sourceAgent.Relations.ToDictionary(agent => agent.Name, agent => agent.Clone());
            PublicKey = sourceAgent.PublicKey;
        }

        protected IAgent GetRelation(string name)
            => _relations.TryGetValue(name, out var peer) ? peer : null;
    }
}