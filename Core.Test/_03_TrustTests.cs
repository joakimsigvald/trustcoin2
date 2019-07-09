using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class TrustTests : TestBase
    {
        [Fact]
        public void AfterConnectedAgent_TrustIs_BaseTrust()
        {
            MyActor.Connect(OtherAccountName);
            Assert.Equal(0f, MyAccount.GetTrust(OtherAccountName));
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
            MyActor.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trust);
            Assert.Equal(trust, MyAccount.GetTrust(OtherAccountName));
        }

        [Theory]
        [InlineData(-1.001)]
        [InlineData(1.001)]
        public void WhenSetTrustOutOfBounds_ThrowsOutOfBoundsException(float trust)
        {
            MyActor.Connect(OtherAccountName);
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
            MyActor.Connect(OtherAccountName);
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
            MyActor.Connect(OtherAccountName);
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
            MyActor.Connect(OtherAccountName);
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
            MyActor.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.DecreaseTrust(OtherAccountName, (Weight)factor));
        }

        [Fact]
        public void Account_TrustForSelfIsMax()
        {
            Assert.Equal(SignedWeight.Max, MyAccount.Self.Trust);
        }
    }
}