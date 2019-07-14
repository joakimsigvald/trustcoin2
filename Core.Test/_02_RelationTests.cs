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
            MyActor.Connect(OtherAccountName);
            Assert.NotNull(MyAccount.GetPeer(OtherAccountName));
        }

        [Fact]
        public void AfterIConnectWithAgent_IAmConnectedToAgent()
        {
            MyActor.Connect(OtherAccountName);
            Assert.True(MyAccount.IsConnectedTo(OtherAccountName));
        }

        [Fact]
        public void AfterIConnectWithAgent_AgentIsNotConnectedToMe()
        {
            MyActor.Connect(OtherAccountName);
            Assert.False(OtherAccount.IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void AfterMyPeerConnectWithMe_IAmUpdatedOfPeerConnection()
        {
            MyActor.Connect(OtherAccountName);
            Assert.False(MyAccount.GetPeer(OtherAccountName).IsConnectedTo(MyAccountName));
            OtherActor.Connect(MyAccountName);
            Assert.True(MyAccount.GetPeer(OtherAccountName).IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void AfterMyPeerConnectWithOtherAgent_IAmUpdateOfPeerConnection()
        {
            MyActor.Connect(OtherAccountName);
            OtherActor.Connect(MyAccountName);

            MyActor.Connect(ThirdAccountName);

            Assert.True(OtherAccount.GetPeer(MyAccountName).IsConnectedTo(ThirdAccountName));
        }

        [Fact]
        public void AfterInterconnectedAgents_RelationStrengthsAre_Zero()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
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
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)setWeight);

            Assert.Equal(setWeight, MyAccount.GetRelationWeight(OtherAccountName, ThirdAccountName));
        }

        [Fact]
        public void AfterICreatedChild_ChildIsConnectedToMe()
        {
            const string childName = "child";
            Interconnect(MyActor, OtherActor);

            var newAccount = MyActor.CreateAccount(childName);

            Assert.True(newAccount.IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void WhenPeerCreateChild_IConnectWithChild()
        {
            const string childName = "child";
            Interconnect(MyActor, OtherActor);

            OtherActor.CreateAccount(childName);

            Assert.True(OtherAccount.IsConnectedTo(childName));
        }
    }
}