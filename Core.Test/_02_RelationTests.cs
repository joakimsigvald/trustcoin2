using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class RelationTests : TestBase
    {
        [Fact]
        public void SelfIsConnectedToSelf()
        {
            Assert.True(MyAccount.IsConnectedTo(MyAccountName));
            Assert.True(MyAccount.Self.IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void SelfHasMaxRelationToSelf()
        {
            Assert.Equal((Weight)1, MyAccount.Self.GetRelation(MyAccount.Name).Strength);
        }

        [Fact]
        public void WhenConnectAgent_ItBecomesPeer()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.NotNull(MyAccount.GetPeer(OtherAccountName));
        }

        [Fact]
        public void AfterIConnectWithAgent_IAmConnectedToAgent()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.True(MyAccount.IsConnectedTo(OtherAccountName));
        }

        [Fact]
        public void AfterIConnectWithAgent_AgentIsNotConnectedToMe()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.False(OtherAccount.IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void AfterMyPeerConnectWithMe_IAmUpdatedOfPeerConnection()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.False(MyAccount.GetPeer(OtherAccountName).IsConnectedTo(MyAccountName));
            OtherAccount.Connect(MyAccountName);
            Assert.True(MyAccount.GetPeer(OtherAccountName).IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void AfterMyPeerConnectWithOtherAgent_IAmUpdateOfPeerConnection()
        {
            MyAccount.Connect(OtherAccountName);
            OtherAccount.Connect(MyAccountName);

            MyAccount.Connect(ThirdAccountName);

            Assert.True(OtherAccount.GetPeer(MyAccountName).IsConnectedTo(ThirdAccountName));
        }

        [Fact]
        public void AfterInterconnectedAgents_RelationStrengthsAre_Zero()
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            var otherToMe = MyAccount.GetPeer(OtherAccountName)
                .GetRelation(MyAccountName);
            var thirdToMe = MyAccount.GetPeer(ThirdAccountName)
                .GetRelation(MyAccountName);
            var otherToThird = MyAccount.GetPeer(OtherAccountName)
                .GetRelation(ThirdAccountName);
            var thirdToOther = MyAccount.GetPeer(ThirdAccountName)
                .GetRelation(OtherAccountName);

            Assert.Equal(0f, otherToMe.Strength);
            Assert.Equal(0f, thirdToMe.Strength);
            Assert.Equal(0f, otherToThird.Strength);
            Assert.Equal(0f, thirdToOther.Strength);
        }


        [Theory]
        [InlineData(0.3)]
        [InlineData(0.7)]
        public void AccountCanSetAndGetRelationWeightForPeers(float setWeight)
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)setWeight);

            Assert.Equal(setWeight, MyAccount.GetRelationWeight(OtherAccountName, ThirdAccountName));
        }
    }
}