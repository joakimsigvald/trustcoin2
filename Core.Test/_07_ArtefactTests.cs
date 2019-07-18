using System.Linq;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Test
{
    public class ArtefactTests : TestBase
    {
        [Fact]
        public void AfterAddArtefactToSelf_SelfHasArtefact()
        {
            MyAccount.Self.AddArtefact(new Artefact(Artefact, MyId));
            Assert.True(MyAccount.Self.HasArtefact(Artefact.Id));
            Assert.Equal(Artefact.Id, MyAccount.Self.GetArtefact(Artefact.Id).Id);
        }

        [Fact]
        public void AfterCreateArtefact_SelfHasArtefact()
        {
            var artefact = MyActor.CreateArtefact(Artefact.Name);
            Assert.True(MyAccount.Self.HasArtefact(artefact.Id));
        }

        [Fact]
        public void WhenAddSameArtefactTwiceToPeer_ThrowsDuplicateKeyException()
        {
            var artefact = new Artefact(Artefact, MyId);
            MyAccount.Self.AddArtefact(artefact);

            var ex = Assert.Throws<DuplicateKey<ArtefactId>>(() => MyAccount.Self.AddArtefact(artefact));
            Assert.Contains($"{artefact.Id}", ex.Message);
        }

        [Fact]
        public void AfterCreateArtefact_MyPeersKnowIHaveIt()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = MyActor.CreateArtefact(Artefact.Name);
            Assert.True(OtherAccount.GetPeer(MyId).HasArtefact(artefact.Id));
        }

        [Fact]
        public void CanCreateTwoDifferentArtefacts()
        {
            Interconnect(MyActor, OtherActor);

            var a1 = MyActor.CreateArtefact(Artefact.Name);
            var a2 = MyActor.CreateArtefact(AnotherArtefact.Name);

            Assert.True(OtherAccount.GetPeer(MyId).HasArtefact(a1.Id));
            Assert.True(OtherAccount.GetPeer(MyId).HasArtefact(a2.Id));
        }

        [Fact]
        public void AfterOtherAccountCreateAndDestroyArtefact_ICanCreateSameArtefact()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var a1 = OtherActor.CreateArtefact(Artefact.Name);
            OtherActor.DestroyArtefact(a1.Id);

            var a2 = MyActor.CreateArtefact(Artefact.Name);

            Assert.True(ThirdAccount.GetPeer(MyId).HasArtefact(a2.Id));
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
            MyAccount.SetTrust(OtherId, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherId, ThirdId, (Weight)relationOfEndorcingPeerToEndorcedPeer);
            var artefact = ThirdActor.CreateArtefact(Artefact.Name);

            OtherActor.EndorceArtefact(artefact);

            Assert.Equal(expectedIncrease, MyAccount.GetMoney(ThirdId));
        }

        [Fact]
        public void WhenEndorceArtefactMayTimes_MoneyIncreaseWithLessThan_1()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, SignedWeight.Max);
            var artefact = ThirdActor.CreateArtefact(Artefact.Name);

            for (int i = 0; i < 100; i++)
                MyActor.EndorceArtefact(artefact);
            Assert.InRange(MyAccount.GetMoney(ThirdId), 0.5f, 1);
        }

        [Fact]
        public void WhenEndorceDifferentArtefactsWithSameOwner_MoneyIncreaseWithLessThan_1()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, SignedWeight.Max);
            var artefacts = Enumerable.Range(1, 100).Select(i => ThirdActor.CreateArtefact(Artefact.Name + i));

            foreach (Artefact artefact in artefacts)
                MyActor.EndorceArtefact(artefact);
            Assert.InRange(MyAccount.GetMoney(ThirdId), 0.5f, 1);
        }

        //Trust

        [Fact]
        public void WhenPeerCreateArtefactThatOtherPeerAlreadyHas_PeerLoosesTrust()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var otherTrustBefore = MyAccount.GetTrust(OtherId);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdId);

            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            ThirdActor.CounterfeitArtefact(artefact);

            Assert.Equal(otherTrustBefore, MyAccount.GetTrust(OtherId));
            var expectedThirdTrust = thirdTrustBefore.Decrease(MakeCounterfeitArtefactDistrustFactor);

            Assert.Equal(expectedThirdTrust, MyAccount.GetTrust(ThirdId));
        }

        [Fact]
        public void WhenPeerDestroyArtefactThatPeerDoesntOwn_PeerLoosesTrust()
        {
            var artefact = MyActor.CreateArtefact(Artefact.Name);
            OtherActor.CounterfeitArtefact(artefact);
            Interconnect(MyActor, OtherActor);
            var trustBefore = MyAccount.GetTrust(OtherId);
            OtherActor.DestroyArtefact(artefact.Id);

            var expectedTrustAfter = trustBefore.Decrease(DestroyOthersArtefactDistrustFactor);

            Assert.Equal(expectedTrustAfter, MyAccount.GetTrust(OtherId));
        }

        [Fact]
        public void AfterEndorcedArtefact_OwnerGainsTrust()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            var trustBefore = MyAccount.GetTrust(OtherId);

            MyActor.EndorceArtefact(artefact);

            var expectedTrust = trustBefore.Increase(ArtefactEndorcementTrustFactor);
            Assert.Equal(expectedTrust, MyAccount.GetTrust(OtherId));
        }

        [Fact]
        public void WhenPeerEndorceArtefactWithWrongOwner_PeerLoosesTrust()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var artefact = ThirdActor.CreateArtefact(Artefact.Name);
            var trustBefore = MyAccount.GetTrust(OtherId);
            OtherAccount.ForgetArtefact(artefact.Id);

            var fakeArtefact = new Artefact(artefact, OtherId);
            OtherActor.EndorceArtefact(fakeArtefact);

            var expectedTrust = trustBefore.Decrease(EndorceCounterfeitArtefactDistrustFactor);
            Assert.Equal(expectedTrust, MyAccount.GetTrust(OtherId));
        }

        // Relation

        [Fact]
        public void WhenPeerEndorceArtefact_PeerRelationWithOwnerIsStrengthened()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            var strengthBefore = MyAccount.Self.GetRelation(OtherId).Strength;

            MyActor.EndorceArtefact(artefact);

            var expectedStrength = strengthBefore.Increase(ArtefactEndorcementTrustFactor);
            var strengthAfter = MyAccount.Self.GetRelation(OtherId).Strength;
            Assert.Equal(expectedStrength, strengthAfter);
        }
    }
}