using Moq;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;
using Xunit;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Test
{
    public class TransactionTests : TestBase
    {
        [Fact]
        public void AfterPeerTransferArtefactToMe_AllMyPeersKnowIHaveIt()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);

            MakeTransaction(OtherActor, MyActor, OtherActor.CreateArtefact(ArtefactName));

            Assert.Equal(MyAccountName, MyAccount.GetArtefact(ArtefactName).OwnerName);
            Assert.Equal(MyAccountName, OtherAccount.GetArtefact(ArtefactName).OwnerName);
            Assert.Equal(MyAccountName, ThirdAccount.GetArtefact(ArtefactName).OwnerName);
        }

        [Fact]
        public void AfterPeerTransferArtefactToMe_AllPeersPeersKnowIHaveIt()
        {
            Interconnect(MyActor, OtherActor);
            Interconnect(OtherActor, ThirdActor);

            MakeTransaction(OtherActor, MyActor, OtherActor.CreateArtefact(ArtefactName));

            Assert.Equal(MyAccountName, ThirdAccount.GetArtefact(ArtefactName).OwnerName);
        }

        [Fact]
        public void WhenTryToUseSameTransactionKeyTwice_NothingHappens()
        {
            var sameKeyFactoryMock = new Mock<ITransactionFactory>();
            sameKeyFactoryMock.SetReturnsDefault("X");
            var myActor = MyAccount.GetActor(_network, sameKeyFactoryMock.Object);
            var otherActor = OtherAccount.GetActor(_network, sameKeyFactoryMock.Object);
            Interconnect(myActor, otherActor);

            MakeTransaction(otherActor, myActor, otherActor.CreateArtefact(ArtefactName));
            MakeTransaction(myActor, otherActor, MyAccount.GetArtefact(ArtefactName));

            Assert.True(MyAccount.Self.HasArtefact(ArtefactName));
            Assert.False(OtherAccount.Self.HasArtefact(ArtefactName));
        }

        [Fact]
        public void IfThreeOfFourPeersAgreeGiverHasArtefact_TransactionCanBeAccepted_ButIsNotAcceptedByDisagreeingPeer()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            ThirdActor.CreateArtefact(ArtefactName);
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)0.5f);
            MyAccount.SetTrust(ThirdAccountName, (SignedWeight)0.5f);

            Assert.True(MakeTransaction(OtherActor, MyActor, artefact));
            Assert.Equal(ThirdAccountName, ThirdAccount.GetArtefact(ArtefactName).OwnerName);
        }

        [Fact]
        public void IfTwoOfThreePeersAgreeGiverHasArtefact_ItCannotBeAcceptedAndIsNotAccounted()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            ThirdActor.CreateArtefact(ArtefactName);
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)1);
            MyAccount.SetTrust(ThirdAccountName, (SignedWeight)1);

            Assert.False(MakeTransaction(OtherActor, MyActor, artefact));
            Assert.False(MyAccount.Self.HasArtefact(ArtefactName));
        }

        [Fact]
        public void EvenIfSomePeersDontKnowArtefact_TransactionIsVerified()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherAccountName, (SignedWeight)1);
            MyAccount.SetTrust(ThirdAccountName, (SignedWeight)1);
        }

        [Fact]
        public void WhenTransactionIsNotAccounted_InvolvedPeersLoseTrust()
        {
            Interconnect(OtherActor, ThirdActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            MyActor.CreateArtefact(ArtefactName);
            Interconnect(MyActor, OtherActor, ThirdActor);
            var otherTrustBefore = MyAccount.GetTrust(OtherAccountName);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdAccountName);

            MakeTransaction(OtherActor, ThirdActor, artefact);

            var expectedOtherTrust = otherTrustBefore.Decrease(UnaccountedTransactionDistrustFactor);
            var expectedThirdTrust = thirdTrustBefore.Decrease(UnaccountedTransactionDistrustFactor);
            var actualOtherTrust = MyAccount.GetTrust(OtherAccountName);
            var actualThirdTrust = MyAccount.GetTrust(ThirdAccountName);
            Assert.Equal(expectedOtherTrust, actualOtherTrust);
            Assert.Equal(expectedThirdTrust, actualThirdTrust);
        }

        [Fact]
        public void WhenTransactionIsAccounted_InvolvedPeersGainTrust()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            var otherTrustBefore = MyAccount.GetTrust(OtherAccountName);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdAccountName);

            MakeTransaction(OtherActor, ThirdActor, artefact);

            var expectedOtherTrust = otherTrustBefore.Increase(AccountedTransactionTrustFactor);
            var expectedThirdTrust = thirdTrustBefore.Increase(AccountedTransactionTrustFactor);
            var actualOtherTrust = MyAccount.GetTrust(OtherAccountName);
            var actualThirdTrust = MyAccount.GetTrust(ThirdAccountName);
            Assert.Equal(expectedOtherTrust, actualOtherTrust);
            Assert.Equal(expectedThirdTrust, actualThirdTrust);
        }

        private bool MakeTransaction(IActor giver, IActor receiver, IArtefact artefact)
        {
            var key = giver.StartTransaction(receiver.Account.Name, artefact);
            return receiver.AcceptTransaction(key);
        }
    }
}