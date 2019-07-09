using Trustcoin.Core.Entities;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Test
{
    public class ArtefactTests : TestBase
    {
        private const string ArtefactName = "SomeArtefact";
        private const string AnotherArtefactName = "AnotherArtefact";

        [Fact]
        public void AfterAddArtefactToSelf_SelfHasArtefact()
        {
            MyAccount.Self.AddArtefact(new Artefact(ArtefactName, MyAccountName));
            Assert.True(MyAccount.Self.HasArtefact(ArtefactName));
        }

        [Fact]
        public void AfterAddArtefactToSelf_SelfCanGetArtefact()
        {
            MyAccount.Self.AddArtefact(new Artefact(ArtefactName, MyAccountName));
            Assert.Equal(ArtefactName, MyAccount.Self.GetArtefact(ArtefactName).Name);
        }

        [Fact]
        public void AfterCreateArtefact_SelfHasArtefact()
        {
            MyActor.CreateArtefact(ArtefactName);
            Assert.True(MyAccount.Self.HasArtefact(ArtefactName));
        }

        [Fact]
        public void WhenAddSameArtefactTwiceToPeer_ThrowsDuplicateKeyException()
        {
            var artefact = new Artefact(ArtefactName, MyAccountName);
            MyAccount.Self.AddArtefact(artefact);
            var ex = Assert.Throws<DuplicateKey<string>>(() => MyAccount.Self.AddArtefact(artefact));
            Assert.Contains(ArtefactName, ex.Message);
        }

        [Fact]
        public void AfterCreateArtefact_MyPeersKnowIHaveIt()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyActor.CreateArtefact(ArtefactName);
            Assert.True(OtherAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
            Assert.True(ThirdAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
        }

        [Fact]
        public void CanCreateTwoDifferentArtefacts()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyActor.CreateArtefact(ArtefactName);
            MyActor.CreateArtefact(AnotherArtefactName);
            Assert.True(OtherAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
            Assert.True(ThirdAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
            Assert.True(OtherAccount.GetPeer(MyAccountName).HasArtefact(AnotherArtefactName));
            Assert.True(ThirdAccount.GetPeer(MyAccountName).HasArtefact(AnotherArtefactName));
        }

        [Fact]
        public void AfterOtherAccountCreateAndDestroyArtefact_ICanCreateSameArtefact()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            OtherActor.CreateArtefact(ArtefactName);
            OtherActor.DestroyArtefact(ArtefactName);

            MyActor.CreateArtefact(ArtefactName);

            Assert.True(ThirdAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
        }

        //Money

        [Theory]
        [InlineData(1, 0, 0.01)]
        [InlineData(0.5, 0, 0.005)]
        [InlineData(0, 0, 0)]
        [InlineData(-0.5, 0, 0)]
        [InlineData(1, 0.5, 0.005)]
        [InlineData(0.5, 0.5, 0.0025)]
        public void AfterPeerEndorceArtefact_ArtefactOwnerIncreaseMoney(
            float trustForEndorcingPeer,
            float relationOfEndorcingPeerToEndorcedPeer,
            float expectedIncrease)
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)relationOfEndorcingPeerToEndorcedPeer);
            var artefact = ThirdActor.CreateArtefact(ArtefactName);

            OtherActor.EndorceArtefact(artefact);

            Assert.Equal(expectedIncrease, MyAccount.GetMoney(ThirdAccountName));
        }

        [Fact]
        public void WhenEndorceArtefactMayTimes_MoneyIncreaseWithLessThan_1()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherAccountName, SignedWeight.Max);
            var artefact = ThirdActor.CreateArtefact(ArtefactName);

            for (int i = 0; i < 10; i++)
                MyActor.EndorceArtefact(artefact);
            Assert.InRange((float)MyAccount.GetMoney(ThirdAccountName), 0, 1);
        }

        //Trust

        [Fact]
        public void WhenPeerCreateArtefactThatOtherPeerAlreadyHas_PeerLoosesTrust()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var otherTrustBefore = MyAccount.GetTrust(OtherAccountName);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdAccountName);

            OtherActor.CreateArtefact(ArtefactName);
            ThirdActor.CreateArtefact(ArtefactName);

            Assert.Equal(otherTrustBefore, MyAccount.GetTrust(OtherAccountName));
            var expectedThirdTrust = thirdTrustBefore.Decrease(CounterfeitArtefactDistrustFactor);

            Assert.Equal(expectedThirdTrust, MyAccount.GetTrust(ThirdAccountName));
        }

        [Fact]
        public void WhenPeerDestroyArtefactThatPeerDoesntOwn_PeerLoosesTrust()
        {
            MyActor.CreateArtefact(ArtefactName);
            OtherActor.CreateArtefact(ArtefactName);
            Interconnect(MyActor, OtherActor);
            var trustBefore = MyAccount.GetTrust(OtherAccountName);
            OtherActor.DestroyArtefact(ArtefactName);

            var expectedTrustAfter = trustBefore.Decrease(DestroyOthersArtefactDistrustFactor);

            Assert.Equal(expectedTrustAfter, MyAccount.GetTrust(OtherAccountName));
        }

        [Fact]
        public void AfterEndorcedArtefact_OwnerGainsTrust()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            var trustBefore = MyAccount.GetTrust(OtherAccountName);

            MyActor.EndorceArtefact(artefact);

            var expectedTrust = trustBefore.Increase(ArtefactEndorcementTrustFactor);
            Assert.Equal(expectedTrust, MyAccount.GetTrust(OtherAccountName));
        }

        [Fact]
        public void WhenPeerEndorceArtefactWithWrongOwner_PeerLoosesTrust()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var artefact = ThirdActor.CreateArtefact(ArtefactName);
            var trustBefore = MyAccount.GetTrust(OtherAccountName);
            OtherAccount.ForgetArtefact(ArtefactName);

            var fakeArtefact = new Artefact(artefact.Name, OtherAccountName);
            OtherActor.EndorceArtefact(fakeArtefact);

            var expectedTrust = trustBefore.Decrease(EndorceCounterfeitArtefactDistrustFactor);
            Assert.Equal(expectedTrust, MyAccount.GetTrust(OtherAccountName));
        }

        // Relation

        [Fact]
        public void WhenPeerEndorceArtefact_PeerRelationWithOwnerIsStrengthened()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            var strengthBefore = MyAccount.Self.GetRelation(OtherAccountName).Strength;

            MyActor.EndorceArtefact(artefact);

            var expectedStrength = strengthBefore.Increase(ArtefactEndorcementTrustFactor);
            var strengthAfter = MyAccount.Self.GetRelation(OtherAccountName).Strength;
            Assert.Equal(expectedStrength, strengthAfter);
        }
    }
}