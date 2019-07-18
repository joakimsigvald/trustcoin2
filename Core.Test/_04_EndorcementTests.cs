using Trustcoin.Core.Types;
using Xunit;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Test
{
    public class EndorcementTests : TestBase
    {
        [Fact]
        public void AfterIEndorcedUnconnectedAgent_IAmConnectedToAgent()
        {
            MyActor.Endorce(OtherId);
            Assert.True(MyAccount.IsConnectedTo(OtherId));
        }

        [Fact]
        public void AfterEndorcedUnconnectedAgent_RelationIsIncreaseWithEndorcementFactor()
        {
            Interconnect(MyActor, OtherActor);

            OtherActor.Endorce(ThirdId);

            var expectedRelationWeight = Weight.Min.Increase(EndorcementTrustFactor);
            var actualRelationWeight = MyAccount.GetPeer(OtherId).GetRelation(ThirdId).Strength;
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.3)]
        [InlineData(0.6)]
        public void AfterEndorcedConnectedAgent_RelationIsIncreaseWithEndorcementFactor(float initialRelation)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetRelationWeight(OtherId, ThirdId, (Weight)initialRelation);

            OtherActor.Endorce(ThirdId);

            var expectedRelationWeight = ((Weight)initialRelation).Increase(EndorcementTrustFactor);
            var actualRelationWeight = MyAccount.GetRelationWeight(OtherId, ThirdId);
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void AfterIEndorcedPeer_TrustOfPeerIncreaseWithEndorcementFactor(float trustValueBefore)
        {
            var trustBefore = (SignedWeight)trustValueBefore;
            MyActor.Connect(OtherId);
            MyAccount.SetTrust(OtherId, trustBefore);

            MyActor.Endorce(OtherId);

            Assert.Equal(trustBefore.Increase(EndorcementTrustFactor), MyAccount.GetTrust(OtherId));
        }
    }
}