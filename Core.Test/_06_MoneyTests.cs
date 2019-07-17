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
            MyActor.Connect(OtherName);
            Assert.Equal((Money)0, MyAccount.GetPeer(OtherName).Money);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 0)]
        [InlineData(2, 3, 3)]
        public void AfterSetMoney_PeerHasMoney(float initialMoney, float setMoney, float hasMoney)
        {
            MyActor.Connect(OtherName);
            MyAccount.SetMoney(OtherName, (Money)initialMoney);

            MyAccount.SetMoney(OtherName, (Money)setMoney);

            Assert.Equal(hasMoney, MyAccount.GetMoney(OtherName));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-0.00001)]
        public void WhenSetNegativeMoney_ThrowsOutOfBounds(float setMoney)
        {
            MyActor.Connect(OtherName);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.SetMoney(OtherName, (Money)setMoney));
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
            MyAccount.SetTrust(OtherName, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherName, ThirdName, (Weight)relationOfEndorcingPeerToEndorcedPeer);

            OtherActor.Endorce(ThirdName);

            Assert.Equal(expectedIncrease, MyAccount.GetMoney(ThirdName));
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
            MyAccount.SetTrust(OtherName, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherName, MyName, (Weight)relationOfEndorcingPeerToEndorcedPeer);

            OtherActor.Endorce(MyName);
            Assert.Equal(expectedIncrease, MyAccount.Self.Money);
        }

        [Fact]
        public void WhenEndorcePeerManyTimes_MoneyIncreaseLessThanTwo()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherName, SignedWeight.Max);
            for (int i = 0; i < 10; i++)
                OtherActor.Endorce(ThirdName);
            Assert.InRange((float)MyAccount.GetMoney(ThirdName), 0, 2);
        }
    }
}