using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class MoneyTests : TestBase
    {
        [Fact]
        public void NewPeerHasNoMoney()
        {
            MyActor.Connect(OtherId);
            Assert.Equal((Money)0, MyAccount.GetPeer(OtherId).Money);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 0)]
        [InlineData(2, 3, 3)]
        public void AfterSetMoney_PeerHasMoney(float initialMoney, float setMoney, float hasMoney)
        {
            MyActor.Connect(OtherId);
            MyAccount.SetMoney(OtherId, (Money)initialMoney);

            MyAccount.SetMoney(OtherId, (Money)setMoney);

            Assert.Equal(hasMoney, MyAccount.GetMoney(OtherId));
        }

        [Fact]
        public void WhenSetNegativeMoney_ThrowsOutOfBounds()
        {
            MyActor.Connect(OtherId);
            Assert.Throws<OutOfBounds<float>>(() => MyAccount.SetMoney(OtherId, (Money)(-0.00001f)));
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
            float expectedMoney)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherId, ThirdId, (Weight)relationOfEndorcingPeerToEndorcedPeer);

            OtherActor.Endorce(ThirdId);

            Assert.Equal(expectedMoney, MyAccount.GetMoney(ThirdId));
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
            float expectedMoney)
        {
            Interconnect(MyActor, OtherActor);
            MyAccount.SetTrust(OtherId, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherId, MyId, (Weight)relationOfEndorcingPeerToEndorcedPeer);

            OtherActor.Endorce(MyId);
            Assert.Equal(expectedMoney, MyAccount.Self.Money);
        }

        [Fact]
        public void WhenEndorcePeerManyTimes_MoneyIncreaseLessThanTwo()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, SignedWeight.Max);
            for (int i = 0; i < 100; i++)
                OtherActor.Endorce(ThirdId);
            Assert.InRange(MyAccount.GetMoney(ThirdId), 1f, 2f);
        }
    }
}