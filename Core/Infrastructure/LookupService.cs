using System.Collections.Generic;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
{
    internal class LookupService : ILookupService
    {
        readonly IDictionary<AgentId, IAgent> _agents = new Dictionary<AgentId, IAgent>();

        public void Add(IAgent agent)
        {
            _agents.Add(agent.Id, agent);
        }

        public IAgent Find(AgentId id)
            => _agents.TryGetValue(id, out var agent) ? agent : null;
    }
}