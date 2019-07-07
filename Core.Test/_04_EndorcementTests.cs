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
            MyAccount.Endorce(OtherAccountName);
            Assert.NotNull(MyAccount.GetPeer(OtherAccountName));
        }

        [Fact]
        public void WhenEndorceAgent_PeerIsEndorced()
        {
            MyAccount.Endorce(OtherAccountName);
            Assert.True(MyAccount.Self.Endorces(OtherAccountName));
        }

        [Fact]
        public void WhenEndorceEndorcedPeer_PeerIsStillEndorced()
        {
            MyAccount.Endorce(OtherAccountName);
            MyAccount.Endorce(OtherAccountName);
            Assert.True(MyAccount.Self.Endorces(OtherAccountName));
        }

        [Fact]
        public void Self_IsEndorced()
        {
            Assert.True(MyAccount.Self.IsEndorced);
        }

        [Fact]
        public void AfterEndorcedUnconnectedAgent_RelationIsIncreaseWithEndorcementFactor()
        {
            Interconnect(MyAccount, OtherAccount);

            OtherAccount.Endorce(ThirdAccountName);

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
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)initialRelation);

            OtherAccount.Endorce(ThirdAccountName);

            var expectedRelationWeight = ((Weight)initialRelation).Increase(EndorcementTrustFactor);
            var actualRelationWeight = MyAccount.GetRelationWeight(OtherAccountName, ThirdAccountName);
            Assert.Equal(expectedRelationWeight, actualRelationWeight);
        }

        [Fact]
        public void AfterPeerEndorceMe_IAmUpdatedOfEndorcement()
        {
            MyAccount.Connect(OtherAccountName);
            OtherAccount.Connect(MyAccountName);

            OtherAccount.Endorce(MyAccountName);

            Assert.True(MyAccount.GetPeer(OtherAccountName).Endorces(MyAccountName));
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void AfterEndorcedUnconnectedAgent_TrustOfPeerIncreaseWithEndorcementFactor(float trustValueBefore)
        {
            var trustBefore = (SignedWeight)trustValueBefore;
            MyAccount.Connect(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);

            MyAccount.Endorce(OtherAccountName);

            Assert.Equal(trustBefore.Increase(EndorcementTrustFactor), MyAccount.GetTrust(OtherAccountName));
        }

        [Theory]
        [InlineData(0.1)]
        [InlineData(0.5)]
        [InlineData(-0.5)]
        public void WhenEndorceEndorcedPeer_TrustIsUnchanged(float trustValueBefore)
        {
            var trustBefore = (SignedWeight)trustValueBefore;
            MyAccount.Endorce(OtherAccountName);
            MyAccount.SetTrust(OtherAccountName, trustBefore);

            MyAccount.Endorce(OtherAccountName);

            Assert.Equal(trustBefore, MyAccount.GetTrust(OtherAccountName));
        }

        [Fact]
        public void When_I_EndorcePeerTwice_Then_I_LooseTrust()
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            MyAccount.Endorce(ThirdAccountName);
            var trustBefore = OtherAccount.GetTrust(MyAccountName);
            var expectedTrustAfter = trustBefore.Decrease(DoubleEndorceDistrustFactor);

            MyAccount.Endorce(ThirdAccountName);

            Assert.Equal(expectedTrustAfter, OtherAccount.GetTrust(MyAccountName));
        }
    }
}