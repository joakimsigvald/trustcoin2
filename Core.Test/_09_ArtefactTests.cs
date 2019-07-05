using Trustcoin.Core;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Exceptions;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class ArtefactTests : TestBase
    {
        private const string ArtefactName = "SomeArtefact";
        private const string AnotherArtefactName = "AnotherArtefact";

        [Fact]
        public void AfterAddArtefactToSelf_SelfHasArtefact()
        {
            MyAccount.Self.AddArtefact(new Artefact(ArtefactName));
            Assert.True(MyAccount.Self.HasArtefact(ArtefactName));
        }

        [Fact]
        public void AfterAddArtefactToSelf_SelfCanGetArtefact()
        {
            MyAccount.Self.AddArtefact(new Artefact(ArtefactName));
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
            var artefact = new Artefact(ArtefactName);
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
    }
}