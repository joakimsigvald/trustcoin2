using System;
using System.Linq;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test.Types
{
    public class AgentIdTests
    {
        [Theory]
        [InlineData("1"
            , 1)]
        [InlineData("255"
            , 255)]
        public void CanCreateRootId(string representation, byte number)
        {
            Assert.Equal(representation, new AgentId(number).ToString());
        }

        [Theory]
        [InlineData("1.1"
            , 1, 1)]
        [InlineData("255.255"
            , 255, 255)]
        [InlineData("255.255.255"
            , 255, 255, 255)]
        [InlineData("1.1.1.1"
            , 1, 1, 1, 1)]
        [InlineData("1.1.1.2"
            , 1, 1, 1, 2)]
        [InlineData("127.127.127.127"
            , 127, 127, 127, 127)]
        [InlineData("255.254.253.252"
            , 255, 254, 253, 252)]
        [InlineData("1.1.1.1.1"
            , 1, 1, 1, 1, 1)]
        [InlineData("1.1.1.1.1.1.1.1" //8
            , 1, 1, 1, 1, 1, 1, 1, 1)]
        [InlineData("1.1.1.1.1.1.1.1.1.1.1.1" //12
            , 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1)]
        [InlineData("1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1" //16
            , 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1)]
        [InlineData("1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1" //24
            , 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1)]
        [InlineData("1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1.1" //32
            , 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1)]
        [InlineData("255.255.255.255.255.255.255.255.15.15.15.15.15.15.15.15" //16
            , 255, 255, 255, 255, 255, 255, 255, 255, 15, 15, 15, 15, 15, 15, 15, 15)]
        [InlineData("255.255.255.255.255.255.255.255.15.15.15.15.15.15.15.15.3.3.3.3.3.3.3.3.3.3.3.3.3.3.3.3" //32
            , 255, 255, 255, 255, 255, 255, 255, 255, 15, 15, 15, 15, 15, 15, 15, 15, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3)]
        public void CanCreateIdFromId(string representation, byte rootId, params int[] parts)
        {
            var root = new AgentId(rootId);
            var descendant = ToBytes(parts).Aggregate(root, (id, p) => id + p);
            Assert.Equal(representation, descendant.ToString());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1, 1, 1, 1, 1, 1, 1, 1, 16)]
        [InlineData(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4)]
        [InlineData(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1)]
        public void WhenCreateWithInvalidParts_ThrowsArgumentException(byte rootId, params int[] parts)
        {
            var byteParts = ToBytes(parts);
            Assert.Throws<ArgumentException>(() => byteParts.Aggregate(new AgentId(rootId), (id, p) => id + p));
        }

        [Theory]
        [InlineData("1", "1", 0)]
        [InlineData("1", "2", 1)]
        [InlineData("1", "3", 2)]
        [InlineData("1", "1.1", 1)]
        [InlineData("1.2", "1", 1)]
        [InlineData("1", "1.2.3", 2)]
        [InlineData("1.1.2", "1.2.2", 3)]
        [InlineData("2", "1.3.3", 3)]
        [InlineData("1.1.1", "1.3.3", 4)]
        public void CanComputeDistance(string sourceId, string targetId, int expectedDistance) {
            var source = AgentId.Parse(sourceId);
            var target = AgentId.Parse(targetId);

            Assert.Equal(expectedDistance, source - target);
        }

        private byte[] ToBytes(int[] parts) => parts.Select(p => (byte)p).ToArray();
    }
}
