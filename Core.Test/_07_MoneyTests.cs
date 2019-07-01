using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class MoneyTests : TestBase
    {
        [Fact]
        public void NewPeerHas_0_Coins()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.Equal((Money)0, MyAccount.GetPeer(OtherAccountName).Money);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 0)]
        [InlineData(2, 3, 3)]
        public void AfterSetMoney_PeerHasMoney(float initialMoney, float setMoney, float hasMoney)
        {
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetMoney(OtherAccountName, (Money)initialMoney);

            MyAccount.SetMoney(OtherAccountName, (Money)setMoney);

            Assert.Equal(hasMoney, MyAccount.GetMoney(OtherAccountName));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-0.00001)]
        public void WhenSetNegativeMoney_ThrowsOutOfBounds(float setMoney)
        {
            MyAccount.Connect(OtherAccountName);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.SetMoney(OtherAccountName, (Money)setMoney));
        }

        [Theory]
        [InlineData(0, 1, 0, 1)]
        [InlineData(0, 0.75, 0, 0.5)]
        [InlineData(0, 0.5, 0, 0)]
        [InlineData(0, 0.25, 0, 0)]
        [InlineData(0, 1, 0.5, 0.5)]
        [InlineData(0, 0.75, 0.5, 0.25)]
        [InlineData(1, 1, 0, 2)]
        [InlineData(2, 0.75, 0.5, 2.25)]
        public void AfterOnePeerEndorceAnotherPeer_EndorcedPeerIncreaseMoney(
            float initialMoney,
            float trustForEndorcingPeer,
            float relationOfEndorcingPeerToEndorcedPeer,
            float expectedIncrease)
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            MyAccount.SetTrust(OtherAccountName, (Weight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)relationOfEndorcingPeerToEndorcedPeer);
            MyAccount.SetMoney(ThirdAccountName, (Money)initialMoney);

            OtherAccount.Endorce(ThirdAccountName);
            Assert.Equal(expectedIncrease, MyAccount.GetPeer(ThirdAccountName).Money);
        }
    }
}