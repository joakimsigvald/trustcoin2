using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Core
{
    public class Agent : IAgent
    {
        public bool IsEndorced { get; protected set; }
        private IDictionary<string, IAgent> _relations = new Dictionary<string, IAgent>();

        protected Agent(string name, IEnumerable<IAgent> relations)
        {
            Name = name;
            _relations = relations.ToDictionary(r => r.Name);
        }

        public Agent(IAccount account)
            : this(account.Name, account.Peers.Select(p => p.Clone()))
        {
        }

        public IAgent Clone()
            => new Agent(Name, Relations)
            {
                IsEndorced = IsEndorced
            };

        public ICollection<IAgent> Relations => _relations.Values;
        public bool IsConnectedTo(string name) => _relations.ContainsKey(name);

        public string Name { get; private set; }

        public bool Endorces(string name) => GetRelation(name)?.IsEndorced ?? false;

        public void UpdateRelations(IAgent sourceAgent)
        {
            _relations = sourceAgent.Relations.ToDictionary(agent => agent.Name, agent => agent.Clone());
        }

        protected IAgent GetRelation(string name)
            => _relations.TryGetValue(name, out var peer) ? peer : null;
    }
}