using System;

namespace Trustcoin.Core.Types
{
    public struct ArtefactId : IEquatable<ArtefactId>
    {
        public readonly AgentId CreatorId;
        public readonly uint Number;

        public ArtefactId(AgentId creatorId, uint number)
        {
            CreatorId = creatorId;
            Number = number;
        }

        public static bool operator ==(ArtefactId left, ArtefactId right) => left.Equals(right);
        public static bool operator !=(ArtefactId left, ArtefactId right) => !left.Equals(right);
        public static explicit operator ArtefactId(string representation) => Parse(representation);

        public override string ToString() => $"{CreatorId}:{Number}";

        private static ArtefactId Parse(string representation)
        {
            var parts = representation.Split(':');
            return new ArtefactId((AgentId)parts[0], uint.Parse(parts[1]));
        }

        public bool Equals(ArtefactId other) => other.CreatorId == CreatorId && other.Number == Number;

        public override bool Equals(object obj) => obj is ArtefactId id && Equals(id);

        public override int GetHashCode() => (int)(CreatorId.GetHashCode() + Number);
    }
}