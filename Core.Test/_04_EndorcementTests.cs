using Trustcoin.Core.Types;
using Xunit;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Test
{
    public class EndorcementTests : TestBase
    {
        [Fact]
        public void AfterEndorcedUnconnectedAgent_AgentIsPeer()
        {
            MyActor.Endorce(OtherAccountName);
            Assert.NotNull(MyAccount.GetPeer(OtherAccountName));
        }

        [Fact]
        public void AfterEndorcedUnconnectedAgent_RelationIsIncreaseWithEndorcementFactor()
        {
            Interconnect(MyActor, OtherActor);

            OtherActor.Endorce(ThirdAccountName);

            var expectedRelationWeight = Weight.Min.Increase(EndorcementTrustFactor);
            var actualRelationWeight = MyAccount.GetPeer(OtherAccountName).GetRelation(ThirdAccountName).Strength;
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.3)]
        [InlineData(0.6)]
        public void AfterEndorcedConnectedAgent_RelationIsIncreaseWithEndorcementFactor(float initialRelation)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)initialRelation);

            OtherActor.Endorce(ThirdAccountName);

            var expectedRelationWeight = ((Weight)initialRelation).Increase(EndorcementTrustFactor);
            var actualRelationWeight = MyAccount.GetRelationWeight(OtherAccountName, ThirdAccountName);
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void AfterEndorcedUnconnectedAgent_TrustOfPeerIncreaseWithEndorcementFactor(float trustValueBefore)
        {
            var trustBefore = (SignedWeight)trustValueBefore;
            MyActor.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);

            MyActor.Endorce(OtherAccountName);

            Assert.Equal(trustBefore.Increase(EndorcementTrustFactor), MyAccount.GetTrust(OtherAccountName));
        }
    }
}