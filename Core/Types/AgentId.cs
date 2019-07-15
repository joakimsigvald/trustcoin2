using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Core.Types
{
    public struct AgentId : IEquatable<AgentId>
    {
        private readonly uint[] _segments;

        public AgentId(byte root)
            : this(new byte[] { root })
        {
        }

        public static AgentId operator +(AgentId id, byte number) => id.CreateChild(number);
        public static ushort operator -(AgentId left, AgentId right) => ComputeDistance(left, right);
        public static explicit operator AgentId(string representation) => Parse(representation);

        public override string ToString() => string.Join('.', ToParts());

        private AgentId CreateChild(byte number) => new AgentId(ToParts().Append(number).ToArray());

        private AgentId(byte[] parts)
        {
            if (!IsValid(parts))
                throw new ArgumentException("Valid pattern: [1..255](1-7) | [1..255](8)[1.15](0-16)");
            _segments = ToSegments(parts);
        }

        private static bool IsValid(byte[] parts)
            => parts.Length < 25 && parts.All(p => p > 0) && parts.Skip(8).All(p => p < 16);

        private static uint[] ToSegments(byte[] parts)
            => Enumerable.Range(0, 2)
            .Select(i => ToSegment(8, parts.Skip(i * 4).Take(4)))
            .Concat(Enumerable.Range(0, 2)
            .Select(i => ToSegment(4, parts.Skip(8 + i * 8).Take(8))))
            .ToArray();

        private static uint ToSegment(byte partSize, IEnumerable<byte> parts)
            => parts
            .Select((p, i) => (p, i))
            .Aggregate((uint)0, (s, t) => s + (uint)(t.p << t.i * partSize));

        private IEnumerable<byte> ToParts()
        {
            int i = 0;
            byte next = (byte)_segments[0];
            do {
                yield return next;
                i++;
                if (i < 8)
                    next = (byte)(_segments[i / 4] >> (byte)((i % 4) * 8));
                else if (i < 24)
                    next = (byte)((_segments[2 + (i - 8) / 8] >> (byte)(i % 8 * 4)) % 16);
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

        public bool Equals(AgentId other) => other._segments.SequenceEqual(_segments);

        public override bool Equals(object obj) => obj is AgentId id && Equals(id);

        public override int GetHashCode() => (int)_segments.Sum(s => s);
    }
}