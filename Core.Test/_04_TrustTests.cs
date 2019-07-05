using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Test
{
    public class TrustTests : TestBase
    {
        [Fact]
        public void AfterConnectedAgent_TrustIs_BaseTrust()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.Equal(BaseTrust, MyAccount.GetTrust(OtherAccountName));
        }

        [Fact]
        public void TrustOfSelf_Is_MaxTrust()
        {
            Assert.Equal(SignedWeight.Max, MyAccount.Self.Trust);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void CanSetAndGetTrustOfPeer(float trustValue)
        {
            var trust = (SignedWeight)trustValue;
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trust);
            Assert.Equal(trust, MyAccount.GetTrust(OtherAccountName));
        }

        [Theory]
        [InlineData(-1.001)]
        [InlineData(1.001)]
        public void WhenSetTrustOutOfBounds_ThrowsOutOfBoundsException(float trust)
        {
            MyAccount.Connect(OtherAccountName);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.SetTrust(OtherAccountName, (SignedWeight)trust));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 0.5)]
        [InlineData(0, 0.5, 0.25)]
        [InlineData(1, 1, 1)]
        [InlineData(-1, 1, -0.5)]
        public void CanIncreaseTrustWithFactor(float trustBefore, float factor, float trustAfter)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustBefore);
            MyAccount.IncreaseTrust(OtherAccountName, (Weight)factor);
            Assert.Equal((SignedWeight)trustAfter, MyAccount.GetTrust(OtherAccountName));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, -0.5)]
        [InlineData(0, 0.5, -0.25)]
        [InlineData(1, 1, 0.5)]
        [InlineData(-1, 1, -1)]
        public void CanDecreaseTrustWithFactor(float trustBefore, float factor, float trustAfter)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustBefore);

            MyAccount.DecreaseTrust(OtherAccountName, (Weight)factor);

            Assert.Equal((SignedWeight)trustAfter, MyAccount.GetTrust(OtherAccountName), 6);
        }

        [Theory]
        [InlineData(0, -0.001)]
        [InlineData(1, -0.001)]
        [InlineData(0, 1.001)]
        [InlineData(-1, 1.001)]
        public void WhenIncreaseTrustWithFactor_OutOfBounds_ThrowsOutOfBoundsException(
            float trustBefore, float factor)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.IncreaseTrust(OtherAccountName, (Weight)factor));
        }

        [Theory]
        [InlineData(0, -0.001)]
        [InlineData(1, -0.001)]
        [InlineData(0, 1.001)]
        [InlineData(-1, 1.001)]
        public void WhenDecreaseTrustWithFactor_OutOfBounds_ThrowsOutOfBoundsException(
            float trustBefore, float factor)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.DecreaseTrust(OtherAccountName, (Weight)factor));
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void AfterEndorcedUnconnectedAgent_TrustOfPeerIncreaseWithEndorcementFactor(float trustValueBefore)
        {
            var trustBefore = (SignedWeight)trustValueBefore;
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);

            MyAccount.Endorce(OtherAccountName);

            Assert.Equal(trustBefore.Increase(EndorcementFactor), MyAccount.GetTrust(OtherAccountName));
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void WhenEndorceEndorcedPeer_TrustIsUnchanged(float trustValueBefore)
        {
            var trustBefore = (SignedWeight)trustValueBefore;
            MyAccount.Endorce(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);

            MyAccount.Endorce(OtherAccountName);

            Assert.Equal(trustBefore, MyAccount.GetTrust(OtherAccountName));
        }
    }
}