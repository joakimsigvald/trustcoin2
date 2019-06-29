using Xunit;

namespace Trustcoin.Core.Test
{
    public class TrustTests : TestBase
    {
        [Fact]
        public void AfterConnectedAgent_TrustIs_BaseTrust()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.Equal(Peer.BaseTrust, MyAccount.GetTrust(OtherAccountName));
        }

        [Fact]
        public void TrustOfSelf_Is_1()
        {
            Assert.Equal(Weight.Max, MyAccount.Self.Trust);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        public void CanSetTrustOfPeer(float trust)
        {
            MyAccount.Connect(OtherAccountName);
            Assert.Equal((Weight)trust, MyAccount.SetTrust(OtherAccountName, trust));
        }

        [Theory]
        [InlineData(-0.001)]
        [InlineData(1.001)]
        public void WhenSetTrustOutOfBounds_ThrowsOutOfBoundsException(float trust)
        {
            MyAccount.Connect(OtherAccountName);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.SetTrust(OtherAccountName, trust));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 1)]
        [InlineData(0, 0.5, 0.5)]
        [InlineData(0.2, 0.25, 0.4)]
        [InlineData(0.2, 0.5, 0.6)]
        [InlineData(0.5, 0.8, 0.9)]
        public void CanIncreaseTrustWithFactor(float trustBefore, float factor, float trustAfter)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);
            MyAccount.IncreaseTrust(OtherAccountName, factor);
            Assert.Equal((Weight)trustAfter, MyAccount.GetTrust(OtherAccountName));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 0)]
        [InlineData(1, 0.1, 0.9)]
        [InlineData(1, 0.5, 0.5)]
        [InlineData(0.2, 0.25, 0.15)]
        [InlineData(0.2, 0.5, 0.1)]
        [InlineData(0.5, 0.8, 0.1)]
        public void CanReduceTrustWithFactor(float trustBefore, float factor, float trustAfter)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);
            MyAccount.ReduceTrust(OtherAccountName, factor);
            Assert.Equal(trustAfter, MyAccount.GetTrust(OtherAccountName), 6);
        }

        [Theory]
        [InlineData(0, -0.001)]
        [InlineData(1, -0.001)]
        [InlineData(0, 1.001)]
        [InlineData(1, 1.001)]
        public void WhenIncreaseTrustWithFactor_OutOfBounds_ThrowsOutOfBoundsException(
            float trustBefore, float factor)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.IncreaseTrust(OtherAccountName, factor));
        }

        [Theory]
        [InlineData(0, -0.001)]
        [InlineData(1, -0.001)]
        [InlineData(0, 1.001)]
        [InlineData(1, 1.001)]
        public void WhenReduceTrustWithFactor_OutOfBounds_ThrowsOutOfBoundsException(
            float trustBefore, float factor)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.ReduceTrust(OtherAccountName, factor));
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        public void AfterEndorcedUnconnectedAgent_TrustOfPeerIncreaseWithFactor_0_5(float trustBefore)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);

            MyAccount.Endorce(OtherAccountName);
            var trustAfterEndorce = MyAccount.GetTrust(OtherAccountName);

            MyAccount.SetTrust(OtherAccountName, trustBefore);
            var expectedTrust = MyAccount.IncreaseTrust(OtherAccountName, 0.5f);
            Assert.Equal(expectedTrust, trustAfterEndorce);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(0.8)]
        public void WhenEndorceEndorcedPeer_TrustIsUnchanged(float trustBefore)
        {
            MyAccount.Endorce(OtherAccountName);
            MyAccount.GetPeer(OtherAccountName).Trust = trustBefore;
            MyAccount.Endorce(OtherAccountName);
            Assert.Equal((Weight)trustBefore, MyAccount.GetTrust(OtherAccountName));
        }
    }
}