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
            MyActor.Connect(OtherAccountName);
            Assert.Equal((Money)0, MyAccount.GetPeer(OtherAccountName).Money);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 0)]
        [InlineData(2, 3, 3)]
        public void AfterSetMoney_PeerHasMoney(float initialMoney, float setMoney, float hasMoney)
        {
            MyActor.Connect(OtherAccountName);
            MyAccount.SetMoney(OtherAccountName, (Money)initialMoney);

            MyAccount.SetMoney(OtherAccountName, (Money)setMoney);

            Assert.Equal(hasMoney, MyAccount.GetMoney(OtherAccountName));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-0.00001)]
        public void WhenSetNegativeMoney_ThrowsOutOfBounds(float setMoney)
        {
            MyActor.Connect(OtherAccountName);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.SetMoney(OtherAccountName, (Money)setMoney));
        }

        [Theory]
        [InlineData(1, 0, 1)]
        [InlineData(1, 0.5, 0.5)]
        [InlineData(0.5, 0, 0.5)]
        [InlineData(0.5, 0.5, 0.25)]
        [InlineData(0, 0, 0)]
        [InlineData(-0.5, 0, 0)]
        public void AfterOnePeerEndorceAnotherPeer_EndorcedPeerIncreaseMoney(
            float trustForEndorcingPeer,
            float relationOfEndorcingPeerToEndorcedPeer,
            float expectedIncrease)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)relationOfEndorcingPeerToEndorcedPeer);

            OtherActor.Endorce(ThirdAccountName);

            Assert.Equal(expectedIncrease, MyAccount.GetMoney(ThirdAccountName));
        }

        [Theory]
        [InlineData(1, 0, 1)]
        [InlineData(1, 0.5, 0.5)]
        [InlineData(0.5, 0, 0.5)]
        [InlineData(0.5, 0.5, 0.25)]
        [InlineData(0, 0, 0)]
        [InlineData(-0.5, 0, 0)]
        public void AfterPeerEndorcedMe_MyMoneyIncrease(
            float trustForEndorcingPeer,
            float relationOfEndorcingPeerToEndorcedPeer,
            float expectedIncrease)
        {
            Interconnect(MyActor, OtherActor);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherAccountName, MyAccountName, (Weight)relationOfEndorcingPeerToEndorcedPeer);

            OtherActor.Endorce(MyAccountName);
            Assert.Equal(expectedIncrease, MyAccount.Self.Money);
        }

        [Fact]
        public void WhenEndorcePeerManyTimes_MoneyIncreaseLessThanTwo()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherAccountName, SignedWeight.Max);
            for (int i = 0; i < 10; i++)
                OtherActor.Endorce(ThirdAccountName);
            Assert.InRange((float)MyAccount.GetMoney(ThirdAccountName), 0, 2);
        }
    }
}