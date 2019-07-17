using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class RelationTests : TestBase
    {
        [Fact]
        public void SelfIsConnectedToSelf()
        {
            Assert.True(MyAccount.IsConnectedTo(MyName));
            Assert.True(MyAccount.Self.IsConnectedTo(MyName));
        }

        [Fact]
        public void SelfHasMaxRelationToSelf()
        {
            Assert.Equal((Weight)1, MyAccount.Self.GetRelation(MyAccount.Name).Strength);
        }

        [Fact]
        public void WhenConnectAgent_ItBecomesPeer()
        {
            MyActor.Connect(OtherName);
            Assert.NotNull(MyAccount.GetPeer(OtherName));
        }

        [Fact]
        public void AfterIConnectWithAgent_IAmConnectedToAgent()
        {
            MyActor.Connect(OtherName);
            Assert.True(MyAccount.IsConnectedTo(OtherName));
        }

        [Fact]
        public void AfterIConnectWithAgent_AgentIsNotConnectedToMe()
        {
            MyActor.Connect(OtherName);
            Assert.False(OtherAccount.IsConnectedTo(MyName));
        }

        [Fact]
        public void AfterMyPeerConnectWithMe_IAmUpdatedOfPeerConnection()
        {
            MyActor.Connect(OtherName);
            Assert.False(MyAccount.GetPeer(OtherName).IsConnectedTo(MyName));
            OtherActor.Connect(MyName);
            Assert.True(MyAccount.GetPeer(OtherName).IsConnectedTo(MyName));
        }

        [Fact]
        public void AfterMyPeerConnectWithOtherAgent_IAmUpdateOfPeerConnection()
        {
            MyActor.Connect(OtherName);
            OtherActor.Connect(MyName);

            MyActor.Connect(ThirdName);

            Assert.True(OtherAccount.GetPeer(MyName).IsConnectedTo(ThirdName));
        }

        [Fact]
        public void AfterInterconnectedAgents_RelationStrengthsAre_Zero()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var otherToMe = MyAccount.GetPeer(OtherName)
                .GetRelation(MyName);
            var thirdToMe = MyAccount.GetPeer(ThirdName)
                .GetRelation(MyName);
            var otherToThird = MyAccount.GetPeer(OtherName)
                .GetRelation(ThirdName);
            var thirdToOther = MyAccount.GetPeer(ThirdName)
                .GetRelation(OtherName);

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
            MyAccount.SetRelationWeight(OtherName, ThirdName, (Weight)setWeight);

            Assert.Equal(setWeight, MyAccount.GetRelationWeight(OtherName, ThirdName));
        }

        [Fact]
        public void AfterICreatedChild_ChildIsConnectedToMe()
        {
            const string childName = "child";
            Interconnect(MyActor, OtherActor);

            var newAccount = MyActor.CreateAccount(childName);

            Assert.True(newAccount.IsConnectedTo(MyName));
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