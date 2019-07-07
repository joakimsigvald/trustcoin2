using System.Collections.Generic;

namespace Trustcoin.Core.Entities
{
    public class Artefact : IArtefact
    {
        private readonly IDictionary<string, IAgent> _endorcers = new Dictionary<string, IAgent>();

        public string Name { get; }
        public string OwnerName { get; }

        public Artefact(string name, string ownerName)
        {
            Name = name;
            OwnerName = ownerName;
        }

        public void AddEndorcer(IAgent peer)
        {
            _endorcers.Add(peer.Name, peer);
        }

        public void RemoveEndorcer(IAgent peer)
        {
            _endorcers.Remove(peer.Name);
        }

        public bool IsEndorcedBy(string agentName)
            => _endorcers.ContainsKey(agentName);
    }
}