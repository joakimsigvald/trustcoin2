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
            MyActor.Connect(OtherName);
            Assert.Equal(0f, MyAccount.GetTrust(OtherName));
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
            MyActor.Connect(OtherName);
            MyAccount.SetTrust(OtherName, trust);
            Assert.Equal(trust, MyAccount.GetTrust(OtherName));
        }

        [Theory]
        [InlineData(-1.001)]
        [InlineData(1.001)]
        public void WhenSetTrustOutOfBounds_ThrowsOutOfBoundsException(float trust)
        {
            MyActor.Connect(OtherName);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.SetTrust(OtherName, (SignedWeight)trust));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 0.5)]
        [InlineData(0, 0.5, 0.25)]
        [InlineData(1, 1, 1)]
        [InlineData(-1, 1, -0.5)]
        public void CanIncreaseTrustWithFactor(float trustBefore, float factor, float trustAfter)
        {
            MyActor.Connect(OtherName);
            MyAccount.SetTrust(OtherName, (SignedWeight)trustBefore);
            MyAccount.IncreaseTrust(OtherName, (Weight)factor);
            Assert.Equal((SignedWeight)trustAfter, MyAccount.GetTrust(OtherName));
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, -0.5)]
        [InlineData(0, 0.5, -0.25)]
        [InlineData(1, 1, 0.5)]
        [InlineData(-1, 1, -1)]
        public void CanDecreaseTrustWithFactor(float trustBefore, float factor, float trustAfter)
        {
            MyActor.Connect(OtherName);
            MyAccount.SetTrust(OtherName, (SignedWeight)trustBefore);

            MyAccount.DecreaseTrust(OtherName, (Weight)factor);

            Assert.Equal((SignedWeight)trustAfter, MyAccount.GetTrust(OtherName), 6);
        }

        [Theory]
        [InlineData(0, -0.001)]
        [InlineData(1, -0.001)]
        [InlineData(0, 1.001)]
        [InlineData(-1, 1.001)]
        public void WhenIncreaseTrustWithFactor_OutOfBounds_ThrowsOutOfBoundsException(
            float trustBefore, float factor)
        {
            MyActor.Connect(OtherName);
            MyAccount.SetTrust(OtherName, (SignedWeight)trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.IncreaseTrust(OtherName, (Weight)factor));
        }

        [Theory]
        [InlineData(0, -0.001)]
        [InlineData(1, -0.001)]
        [InlineData(0, 1.001)]
        [InlineData(-1, 1.001)]
        public void WhenDecreaseTrustWithFactor_OutOfBounds_ThrowsOutOfBoundsException(
            float trustBefore, float factor)
        {
            MyActor.Connect(OtherName);
            MyAccount.SetTrust(OtherName, (SignedWeight)trustBefore);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.DecreaseTrust(OtherName, (Weight)factor));
        }

        [Fact]
        public void Account_TrustForSelfIsMax()
        {
            Assert.Equal(SignedWeight.Max, MyAccount.Self.Trust);
        }
    }
}