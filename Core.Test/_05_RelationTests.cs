using Xunit;
using static Trustcoin.Core.Constants;

namespace Trustcoin.Core.Test
{
    public class RelationTests : TestBase
    {
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

        [Fact]
        public void AfterEndorcedConnectedAgent_RelationIsBaseWeightIncreaseWithEndorcementFactor()
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);

            OtherAccount.Endorce(ThirdAccountName);

            var expectedRelationWeight = BaseRelationWeight.Increase(EndorcementFactor);
            var actualRelationWeight = MyAccount.GetPeer(OtherAccountName).GetRelation(ThirdAccountName).Weight;
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }
    }
}