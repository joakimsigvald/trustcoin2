using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Core.Types
{
    public struct AgentId : IEquatable<AgentId>
    {
        public readonly uint[] Segments;

        public static AgentId None { get; internal set; }

        [JsonConstructor]
        public AgentId(uint[] segments)
        {
            Segments = segments;
        }

        public AgentId(byte root)
            : this(new byte[] { root })
        {
        }

        public static AgentId operator +(AgentId id, byte number) => id.CreateChild(number);
        public static ArtefactId operator /(AgentId id, uint number) 
            => new ArtefactId(id, number);

        public static ushort operator -(AgentId left, AgentId right) => ComputeDistance(left, right);
        public static bool operator ==(AgentId left, AgentId right) => left.Equals(right);
        public static bool operator !=(AgentId left, AgentId right) => !left.Equals(right);
        public static explicit operator AgentId(string representation) => Parse(representation);

        public override string ToString() => string.Join('.', ToParts());

        private AgentId CreateChild(byte number) => new AgentId(ToParts().Append(number).ToArray());

        private AgentId(byte[] parts)
        {
            if (!IsValid(parts))
                throw new ArgumentException("Valid pattern: [1..255](1-7) | [1..255](8)[1.15](0-7) | [1..255](8)[1.15](8)[1.3](0-16)");
            Segments = ToSegments(parts);
        }

        private static bool IsValid(byte[] parts)
            => parts.Length < 33 
            && parts.All(p => p > 0)
            && parts.Skip(8).All(p => p < 16)
            && parts.Skip(16).All(p => p < 4);

        private static uint[] ToSegments(byte[] parts)
            => Enumerable.Range(0, 2)
            .Select(i => ToSegment(8, parts.Skip(i * 4).Take(4)))
            .Append(ToSegment(4, parts.Skip(8).Take(8)))
            .Append(ToSegment(2, parts.Skip(16)))
            .ToArray();

        private static uint ToSegment(byte partSize, IEnumerable<byte> parts)
            => parts
            .Select((p, i) => (p, i))
            .Aggregate((uint)0, (s, t) => s + (uint)(t.p << t.i * partSize));

        private IEnumerable<byte> ToParts()
        {
            int i = 0;
            byte next = (byte)Segments[0];
            do {
                yield return next;
                i++;
                if (i < 8)
                    next = (byte)(Segments[i / 4] >> (byte)((i % 4) * 8));
                else if (i < 16)
                    next = (byte)((Segments[2] >> (byte)(i % 8 * 4)) % 16);
                else if (i < 32)
                    next = (byte)((Segments[3] >> (byte)(i % 16 * 2)) % 4);
                else
                    break;
            }
            while (next > 0);
        }

        public static AgentId Parse(string representation)
            => new AgentId(representation.Split('.').Select(byte.Parse).ToArray());

        private static ushort ComputeDistance(AgentId left, AgentId right)
        {
            var path1 = left.ToParts().ToArray();
            var path2 = right.ToParts().ToArray();
            return GetDistance(path1, path2);
        }

        private static ushort GetDistance(byte[] path1, byte[] path2)
            => path1.Length > path2.Length ? GetDistance(path2, path1)
            : path1.Length == 0 ? (ushort)path2.Length
            : path1[0] == path2[0] ? GetDistance(path1[1..], path2[1..])
            : (ushort)(Math.Abs(path1[0] - path2[0]) + path1.Length + path2.Length - 2);

        public bool Equals(AgentId other) => 
            other.Segments is null 
            ? Segments is null
            : other.Segments.SequenceEqual(Segments);

        public override bool Equals(object obj) => obj is AgentId id && Equals(id);

        public override int GetHashCode() => (int)Segments.Sum(s => s);
    }
}