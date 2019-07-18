using Newtonsoft.Json;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public class Artefact
    {
        public Artefact(Artefact artefact, AgentId ownerId)
            : this(artefact.Id, artefact.Name, ownerId)
        {
        }

        [JsonConstructor]
        public Artefact(ArtefactId id, string name, AgentId ownerId)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
        }

        public ArtefactId Id { get; }
        public string Name { get; }
        public AgentId OwnerId { get; }

        public override string ToString()
            => $"{Id}-{Name}-{OwnerId}";
    }
}