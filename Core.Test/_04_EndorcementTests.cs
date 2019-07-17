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
            MyActor.Endorce(OtherName);
            Assert.NotNull(MyAccount.GetPeer(OtherName));
        }

        [Fact]
        public void AfterEndorcedUnconnectedAgent_RelationIsIncreaseWithEndorcementFactor()
        {
            Interconnect(MyActor, OtherActor);

            OtherActor.Endorce(ThirdName);

            var expectedRelationWeight = Weight.Min.Increase(EndorcementTrustFactor);
            var actualRelationWeight = MyAccount.GetPeer(OtherName).GetRelation(ThirdName).Strength;
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.3)]
        [InlineData(0.6)]
        public void AfterEndorcedConnectedAgent_RelationIsIncreaseWithEndorcementFactor(float initialRelation)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetRelationWeight(OtherName, ThirdName, (Weight)initialRelation);

            OtherActor.Endorce(ThirdName);

            var expectedRelationWeight = ((Weight)initialRelation).Increase(EndorcementTrustFactor);
            var actualRelationWeight = MyAccount.GetRelationWeight(OtherName, ThirdName);
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void AfterEndorcedUnconnectedAgent_TrustOfPeerIncreaseWithEndorcementFactor(float trustValueBefore)
        {
            var trustBefore = (SignedWeight)trustValueBefore;
            MyActor.Connect(OtherName);
            MyAccount.SetTrust(OtherName, trustBefore);

            MyActor.Endorce(OtherName);

            Assert.Equal(trustBefore.Increase(EndorcementTrustFactor), MyAccount.GetTrust(OtherName));
        }
    }
}