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
            MyAccount.CreateArtefact(ArtefactName);
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
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            MyAccount.CreateArtefact(ArtefactName);
            Assert.True(OtherAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
            Assert.True(ThirdAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
        }

        [Fact]
        public void CanCreateTwoDifferentArtefacts()
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            MyAccount.CreateArtefact(ArtefactName);
            MyAccount.CreateArtefact(AnotherArtefactName);
            Assert.True(OtherAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
            Assert.True(ThirdAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
            Assert.True(OtherAccount.GetPeer(MyAccountName).HasArtefact(AnotherArtefactName));
            Assert.True(ThirdAccount.GetPeer(MyAccountName).HasArtefact(AnotherArtefactName));
        }

        [Fact]
        public void WhenPeerCreateArtefactThatOtherPeerAlreadyHas_PeerWillLooseTrust()
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            var otherTrustBefore = MyAccount.GetTrust(OtherAccountName);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdAccountName);

            OtherAccount.CreateArtefact(ArtefactName);
            ThirdAccount.CreateArtefact(ArtefactName);

            Assert.Equal(otherTrustBefore, MyAccount.GetTrust(OtherAccountName));
            var expectedThirdTrust = thirdTrustBefore.Decrease(CounterfeitArtefactFactor);

            Assert.Equal(expectedThirdTrust, MyAccount.GetTrust(ThirdAccountName));
        }

        [Fact]
        public void AfterOtherAccountCreateAndDestroyArtefact_ICanCreateSameArtefact()
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            OtherAccount.CreateArtefact(ArtefactName);
            OtherAccount.DestroyArtefact(ArtefactName);

            MyAccount.CreateArtefact(ArtefactName);

            Assert.True(ThirdAccount.GetPeer(MyAccountName).HasArtefact(ArtefactName));
        }

        [Fact]
        public void WhenPeerDestroyArtefactThatPeerDoesntOwn_PeerLooseTrust()
        {
            MyAccount.CreateArtefact(ArtefactName);
            OtherAccount.CreateArtefact(ArtefactName);
            Interconnect(MyAccount, OtherAccount);
            var trustBefore = MyAccount.GetTrust(OtherAccountName);
            OtherAccount.DestroyArtefact(ArtefactName);

            var expectedTrustAfter = trustBefore.Decrease(DestroyOthersArtefactFactor);

            Assert.Equal(expectedTrustAfter, MyAccount.GetTrust(OtherAccountName));
        }

        [Fact]
        public void WhenEndorceArtefact_ArtefactIsEndorced()
        {
            var artefact = OtherAccount.CreateArtefact(ArtefactName);
            MyAccount.EndorceArtefact(artefact);
            Assert.True(MyAccount.EndorcesArtefact(MyAccount.Name, artefact.Name));
        }

        [Fact]
        public void WhenEndorceEndorcedArtefact_ArtefactIsStillEndorced()
        {
            var artefact = OtherAccount.CreateArtefact(ArtefactName);
            MyAccount.EndorceArtefact(artefact);
            MyAccount.EndorceArtefact(artefact);
            Assert.True(MyAccount.EndorcesArtefact(MyAccount.Name, ArtefactName));
        }

        [Theory]
        [InlineData(1, 0, 0.001)]
        [InlineData(0.5, 0, 0.0005)]
        [InlineData(0, 0, 0)]
        [InlineData(-0.5, 0, 0)]
        [InlineData(1, 0.5, 0.0005)]
        [InlineData(0.5, 0.5, 0.00025)]
        public void AfterPeerEndorceArtefact_ArtefactOwnerIncreaseMoney(
            float trustForEndorcingPeer,
            float relationOfEndorcingPeerToEndorcedPeer,
            float expectedIncrease)
        {
            Interconnect(MyAccount, OtherAccount, ThirdAccount);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)trustForEndorcingPeer);
            MyAccount.SetRelationWeight(OtherAccountName, ThirdAccountName, (Weight)relationOfEndorcingPeerToEndorcedPeer);
            var artefact = ThirdAccount.CreateArtefact(ArtefactName);

            OtherAccount.EndorceArtefact(artefact);

            Assert.Equal(expectedIncrease, MyAccount.GetMoney(ThirdAccountName));
        }

        [Fact]
        public void AfterEndorcedArtefact_TrustOfOwnerIncrease()
        {
            Interconnect(MyAccount, OtherAccount);
            var artefact = OtherAccount.CreateArtefact(ArtefactName);
            var trustBefore = MyAccount.GetTrust(OtherAccountName);

            MyAccount.EndorceArtefact(artefact);

            var expectedTrust = trustBefore.Increase(ArtefactEndorcementFactor);

            Assert.Equal(expectedTrust, MyAccount.GetTrust(OtherAccountName));
        }

        [Fact]
        public void WhenEndorceEndorcedArtefact_TrustOfOwnerIsUnchanged()
        {
            Interconnect(MyAccount, OtherAccount);
            var artefact = OtherAccount.CreateArtefact(ArtefactName);
            MyAccount.EndorceArtefact(artefact);
            var trustBefore = MyAccount.GetTrust(OtherAccountName);

            MyAccount.EndorceArtefact(artefact);

            Assert.Equal(trustBefore, MyAccount.GetTrust(OtherAccountName));
        }
    }
}