using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class RelationTests : TestBase
    {
        [Fact]
        public void SelfIsConnectedToSelf()
        {
            Assert.True(MyAccount.IsConnectedTo(MyId));
            Assert.True(MyAccount.Self.IsConnectedTo(MyId));
        }

        [Fact]
        public void SelfAlwaysHasMaxRelationToSelf()
        {
            MyAccount.SetRelationWeight(MyId, MyId, (Weight)0);
            Assert.Equal((Weight)1, MyAccount.Self.GetRelation(MyId).Strength);
        }

        [Fact]
        public void AfterIConnectWithAgent_IAmConnectedToAgent()
        {
            MyActor.Connect(OtherId);

            Assert.NotNull(MyAccount.GetPeer(OtherId));
            Assert.True(MyAccount.IsConnectedTo(OtherId));
        }

        [Fact]
        public void AfterIConnectWithAgent_AgentIsNotConnectedToMe()
        {
            MyActor.Connect(OtherId);
            Assert.False(OtherAccount.IsConnectedTo(MyId));
            Assert.False(MyAccount.GetPeer(OtherId).IsConnectedTo(MyId));
        }

        [Fact]
        public void AfterMyPeerConnectWithMe_IAmUpdatedOfPeerConnection()
        {
            MyActor.Connect(OtherId);
            OtherActor.Connect(MyId);
            Assert.True(MyAccount.GetPeer(OtherId).IsConnectedTo(MyId));
        }

        [Fact]
        public void AfterMyPeerConnectWithOtherAgent_IAmUpdatedOfPeerConnection()
        {
            Interconnect(MyActor, OtherActor);
            OtherActor.Connect(ThirdId);
            Assert.True(MyAccount.GetPeer(OtherId).IsConnectedTo(ThirdId));
        }

        [Fact]
        public void AfterInterconnectedAgents_RelationStrengthsAre_Zero()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);

            AssertRelationStrength(0, OtherId, MyId);
            AssertRelationStrength(0, ThirdId, MyId);
            AssertRelationStrength(0, OtherId, ThirdId);
            AssertRelationStrength(0, ThirdId, OtherId);
        }

        [Theory]
        [InlineData(0.3)]
        [InlineData(0.7)]
        public void AccountCanSetAndGetRelationWeightForPeers(float weight)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetRelationWeight(OtherId, ThirdId, (Weight)weight);
            Assert.Equal(weight, MyAccount.GetRelationWeight(OtherId, ThirdId));
        }

        [Fact]
        public void AfterICreatedChild_MeAndChildAreInterconnected()
        {
            const string childName = "child";
            Interconnect(MyActor, OtherActor);

            var newAccount = MyActor.CreateAccount(childName);

            Assert.True(newAccount.IsConnectedTo(MyId));
            Assert.True(MyAccount.IsConnectedTo(newAccount.Id));
        }

        [Fact]
        public void WhenMyPeerCreateChild_IConnectWithChild()
        {
            const string childName = "child";
            Interconnect(MyActor, OtherActor);

            var newAccount = OtherActor.CreateAccount(childName);

            Assert.True(MyAccount.IsConnectedTo(newAccount.Id));
        }

        private void AssertRelationStrength(float expected, AgentId from, AgentId to)
        {
            Assert.Equal(expected, MyAccount.GetPeer(from).GetRelation(to).Strength);
        }
    }
}