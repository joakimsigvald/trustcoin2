using System;
using Trustcoin.Core.Types;
using Xunit;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Test
{
    public class RelationTests : TestBase
    {
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
        public void AfterPeerEndorceMe_IAmUpdatedOfEndorcement()
        {
            MyAccount.Connect(OtherAccountName);
            OtherAccount.Connect(MyAccountName);

            OtherAccount.Endorce(MyAccountName);

            Assert.True(MyAccount.GetPeer(OtherAccountName).Endorces(MyAccountName));
        }

        [Fact]
        public void WhenConnectWithSelf_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => MyAccount.Connect(MyAccountName));
        }

        [Fact]
        public void AfterInterconnectedAgents_RelationsAre_BaseRelation()
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

            Assert.Equal(BaseRelationWeight, otherToMe.Weight);
            Assert.Equal(BaseRelationWeight, thirdToMe.Weight);
            Assert.Equal(BaseRelationWeight, otherToThird.Weight);
            Assert.Equal(BaseRelationWeight, thirdToOther.Weight);
        }

        [Fact]
        public void AfterEndorcedUnconnectedAgent_RelationIsBaseWeightIncreaseWithEndorcementFactor()
        {
            Interconnect(MyAccount, OtherAccount);

            OtherAccount.Endorce(ThirdAccountName);

            var expectedRelationWeight = BaseRelationWeight.Increase(EndorcementFactor);
            var actualRelationWeight = MyAccount.GetPeer(OtherAccountName).GetRelation(ThirdAccountName).Weight;
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
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

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.3)]
        [InlineData(0.6)]
        public void AfterEndorcedConnectedAgent_RelationIsIncreaseWithEndorcementFactor(float initialRelation)
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)initialRelation);

            OtherAccount.Endorce(ThirdAccountName);

            var expectedRelationWeight = ((Weight)initialRelation).Increase(EndorcementFactor);
            var actualRelationWeight = MyAccount.GetRelationWeight(OtherAccountName, ThirdAccountName);
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }
    }
}