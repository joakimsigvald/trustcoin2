using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class TrustTests : TestBase
    {
        [Fact]
        public void AfterConnectedAgent_TrustIs_Zero()
        {
            MyActor.Connect(OtherId);
            Assert.Equal(0f, MyAccount.GetTrust(OtherId));
        }

        [Fact]
        public void TrustOfSelf_IsAlways_MaxTrust()
        {
            MyAccount.SetTrust(MyId, (SignedWeight)0);
            MyAccount.DecreaseTrust(MyId, (Weight)1);
            Assert.Equal(SignedWeight.Max, MyAccount.Self.Trust);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void CanSetAndGetTrustOfPeer(float trustValue)
        {
            var trust = (SignedWeight)trustValue;
            MyActor.Connect(OtherId);
            MyAccount.SetTrust(OtherId, trust);
            Assert.Equal(trust, MyAccount.GetTrust(OtherId));
        }

        [Theory]
        [InlineData(-1.001)]
        [InlineData(1.001)]
        public void WhenSetTrustOutOfBounds_ThrowsOutOfBoundsException(float trust)
        {
            MyActor.Connect(OtherId);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.SetTrust(OtherId, (SignedWeight)trust));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 0.5)]
        [InlineData(0, 0.5, 0.25)]
        [InlineData(1, 1, 1)]
        [InlineData(-1, 1, -0.5)]
        public void CanIncreaseTrustWithFactor(float trustBefore, float factor, float trustAfter)
        {
            MyActor.Connect(OtherId);
            MyAccount.SetTrust(OtherId, (SignedWeight)trustBefore);
            MyAccount.IncreaseTrust(OtherId, (Weight)factor);
            Assert.Equal((SignedWeight)trustAfter, MyAccount.GetTrust(OtherId));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, -0.5)]
        [InlineData(0, 0.5, -0.25)]
        [InlineData(1, 1, 0.5)]
        [InlineData(-1, 1, -1)]
        public void CanDecreaseTrustWithFactor(float trustBefore, float factor, float trustAfter)
        {
            MyActor.Connect(OtherId);
            MyAccount.SetTrust(OtherId, (SignedWeight)trustBefore);

            MyAccount.DecreaseTrust(OtherId, (Weight)factor);

            Assert.Equal((SignedWeight)trustAfter, MyAccount.GetTrust(OtherId), 6);
        }

        [Theory]
        [InlineData(0, -0.001)]
        [InlineData(1, -0.001)]
        [InlineData(0, 1.001)]
        [InlineData(-1, 1.001)]
        public void WhenIncreaseTrustWithFactor_OutOfBounds_ThrowsOutOfBoundsException(
            float trustBefore, float factor)
        {
            MyActor.Connect(OtherId);
            MyAccount.SetTrust(OtherId, (SignedWeight)trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.IncreaseTrust(OtherId, (Weight)factor));
        }

        [Theory]
        [InlineData(0, -0.001)]
        [InlineData(1, -0.001)]
        [InlineData(0, 1.001)]
        [InlineData(-1, 1.001)]
        public void WhenDecreaseTrustWithFactor_OutOfBounds_ThrowsOutOfBoundsException(
            float trustBefore, float factor)
        {
            MyActor.Connect(OtherId);
            MyAccount.SetTrust(OtherId, (SignedWeight)trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.DecreaseTrust(OtherId, (Weight)factor));
        }
    }
}