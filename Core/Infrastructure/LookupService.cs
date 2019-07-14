using System.Collections.Generic;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Infrastructure
{
    internal class LookupService : ILookupService
    {
        IDictionary<string, IAgent> _agentsById = new Dictionary<string, IAgent>();
        IDictionary<string, IAgent> _agentsByName = new Dictionary<string, IAgent>();

        public void Add(IAgent agent)
        {
            _agentsById.Add(agent.Id, agent);
            _agentsByName.Add(agent.Name, agent);
        }

        public IAgent FindById(string id)
            => _agentsById.TryGetValue(id, out var agent) ? agent : null;

        public IAgent FindByName(string id)
            => _agentsByName.TryGetValue(id, out var agent) ? agent : null;
    }
}