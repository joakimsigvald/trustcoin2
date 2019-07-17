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
        public void SelfAlwaysHasMaxRelationToSelf()
        {
            MyAccount.SetRelationWeight(MyName, MyName, (Weight)0);
            Assert.Equal((Weight)1, MyAccount.Self.GetRelation(MyAccount.Name).Strength);
        }

        [Fact]
        public void AfterIConnectWithAgent_IAmConnectedToAgent()
        {
            MyActor.Connect(OtherName);

            Assert.NotNull(MyAccount.GetPeer(OtherName));
            Assert.True(MyAccount.IsConnectedTo(OtherName));
        }

        [Fact]
        public void AfterIConnectWithAgent_AgentIsNotConnectedToMe()
        {
            MyActor.Connect(OtherName);
            Assert.False(OtherAccount.IsConnectedTo(MyName));
            Assert.False(MyAccount.GetPeer(OtherName).IsConnectedTo(MyName));
        }

        [Fact]
        public void AfterMyPeerConnectWithMe_IAmUpdatedOfPeerConnection()
        {
            MyActor.Connect(OtherName);
            OtherActor.Connect(MyName);
            Assert.True(MyAccount.GetPeer(OtherName).IsConnectedTo(MyName));
        }

        [Fact]
        public void AfterMyPeerConnectWithOtherAgent_IAmUpdatedOfPeerConnection()
        {
            Interconnect(MyActor, OtherActor);
            OtherActor.Connect(ThirdName);
            Assert.True(MyAccount.GetPeer(OtherName).IsConnectedTo(ThirdName));
        }

        [Fact]
        public void AfterInterconnectedAgents_RelationStrengthsAre_Zero()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);

            AssertRelationStrength(0, OtherName, MyName);
            AssertRelationStrength(0, ThirdName, MyName);
            AssertRelationStrength(0, OtherName, ThirdName);
            AssertRelationStrength(0, ThirdName, OtherName);
        }

        [Theory]
        [InlineData(0.3)]
        [InlineData(0.7)]
        public void AccountCanSetAndGetRelationWeightForPeers(float weight)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetRelationWeight(OtherName, ThirdName, (Weight)weight);
            Assert.Equal(weight, MyAccount.GetRelationWeight(OtherName, ThirdName));
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

            Assert.True(MyAccount.IsConnectedTo(childName));
        }

        private void AssertRelationStrength(float expected, string from, string to)
        {
            Assert.Equal(expected, MyAccount.GetPeer(from).GetRelation(to).Strength);
        }
    }
}