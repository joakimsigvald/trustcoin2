using System.Collections.Generic;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
{
    internal class LookupService : ILookupService
    {
        readonly IDictionary<AgentId, IAgent> _agentsById = new Dictionary<AgentId, IAgent>();
        readonly IDictionary<string, IAgent> _agentsByName = new Dictionary<string, IAgent>();

        public void Add(IAgent agent)
        {
            _agentsById.Add(agent.Id, agent);
            _agentsByName.Add(agent.Name, agent);
        }

        public IAgent Find(AgentId id)
            => _agentsById.TryGetValue(id, out var agent) ? agent : null;

        public IAgent Find(string id)
            => _agentsByName.TryGetValue(id, out var agent) ? agent : null;
    }
}