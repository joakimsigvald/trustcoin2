using Moq;
using System.Collections.Generic;
using System.Linq;
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

            Assert.Equal(MyName, MyAccount.GetArtefact(ArtefactName).OwnerName);
            Assert.Equal(MyName, OtherAccount.GetArtefact(ArtefactName).OwnerName);
            Assert.Equal(MyName, ThirdAccount.GetArtefact(ArtefactName).OwnerName);
        }

        [Fact]
        public void AfterPeerTransferArtefactToMe_AllPeersPeersKnowIHaveIt()
        {
            Interconnect(MyActor, OtherActor);
            Interconnect(OtherActor, ThirdActor);

            MakeTransaction(OtherActor, MyActor, OtherActor.CreateArtefact(ArtefactName));

            Assert.Equal(MyName, ThirdAccount.GetArtefact(ArtefactName).OwnerName);
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
            MyAccount.SetTrust(OtherName, (SignedWeight)0.5f);
            MyAccount.SetTrust(ThirdName, (SignedWeight)0.5f);

            Assert.True(MakeTransaction(OtherActor, MyActor, artefact));
            Assert.Equal(ThirdName, ThirdAccount.GetArtefact(ArtefactName).OwnerName);
        }

        [Fact]
        public void IfTwoOfThreePeersAgreeGiverHasArtefact_ItCannotBeAcceptedAndIsNotAccounted()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            ThirdActor.CreateArtefact(ArtefactName);
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherName, (SignedWeight)1);
            MyAccount.SetTrust(ThirdName, (SignedWeight)1);

            Assert.False(MakeTransaction(OtherActor, MyActor, artefact));
            Assert.False(MyAccount.Self.HasArtefact(ArtefactName));
        }

        [Fact]
        public void EvenIfSomePeersDontKnowArtefact_TransactionIsVerified()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherName, (SignedWeight)1);
            MyAccount.SetTrust(ThirdName, (SignedWeight)1);
        }

        [Fact]
        public void WhenTransactionIsNotAccounted_InvolvedPeersLoseTrust()
        {
            Interconnect(OtherActor, ThirdActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            MyActor.CreateArtefact(ArtefactName);
            Interconnect(MyActor, OtherActor, ThirdActor);
            var otherTrustBefore = MyAccount.GetTrust(OtherName);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdName);

            MakeTransaction(OtherActor, ThirdActor, artefact);

            var expectedOtherTrust = otherTrustBefore.Decrease(UnaccountedTransactionDistrustFactor);
            var expectedThirdTrust = thirdTrustBefore.Decrease(UnaccountedTransactionDistrustFactor);
            var actualOtherTrust = MyAccount.GetTrust(OtherName);
            var actualThirdTrust = MyAccount.GetTrust(ThirdName);
            Assert.Equal(expectedOtherTrust, actualOtherTrust);
            Assert.Equal(expectedThirdTrust, actualThirdTrust);
        }

        [Fact]
        public void WhenTransactionIsAccounted_InvolvedPeersGainTrust()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var artefact = OtherActor.CreateArtefact(ArtefactName);
            var otherTrustBefore = MyAccount.GetTrust(OtherName);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdName);

            MakeTransaction(OtherActor, ThirdActor, artefact);

            var expectedOtherTrust = otherTrustBefore.Increase(AccountedTransactionTrustFactor);
            var expectedThirdTrust = thirdTrustBefore.Increase(AccountedTransactionTrustFactor);
            var actualOtherTrust = MyAccount.GetTrust(OtherName);
            var actualThirdTrust = MyAccount.GetTrust(ThirdName);
            Assert.Equal(expectedOtherTrust, actualOtherTrust);
            Assert.Equal(expectedThirdTrust, actualThirdTrust);
        }

        [Theory]
        [InlineData(0.5, 0.5, true)] // 75% acceept
        [InlineData(1, 0.66, true)] // > 75% acceept
        [InlineData(0.51, 0.51, false)] // < 75% accept
        public void ThreeOfFourPeersMustAgree_ForTransactionToBeAccepted(
            float acceptingPeerTrust, float rejectingPeerTrust, bool transactionAccepted)
        {
            var someMoney = (Money)100;
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherName, (SignedWeight)acceptingPeerTrust);
            MyAccount.SetTrust(ThirdName, (SignedWeight)rejectingPeerTrust);
            MyAccount.SetMoney(OtherName, someMoney);
            OtherAccount.SetTrust(MyName, (SignedWeight)acceptingPeerTrust);
            OtherAccount.SetTrust(ThirdName, (SignedWeight)rejectingPeerTrust);
            OtherAccount.SetMoney(OtherName, someMoney);

            Assert.Equal(0f, MyAccount.GetMoney(MyName));
            Assert.Equal(0f, OtherAccount.GetMoney(MyName));
            Assert.Equal(0f, ThirdAccount.GetMoney(MyName));
            Assert.Equal(someMoney, MyAccount.GetMoney(OtherName));
            Assert.Equal(someMoney, OtherAccount.GetMoney(OtherName));
            Assert.Equal(0f, ThirdAccount.GetMoney(MyName));

            Assert.Equal(transactionAccepted, MakeTransaction(MyActor, OtherActor, someMoney));

            if (!transactionAccepted) return;
            Assert.Equal(someMoney, MyAccount.GetMoney(MyName));
            Assert.Equal(someMoney, OtherAccount.GetMoney(MyName));
            Assert.Equal(0f, ThirdAccount.GetMoney(MyName));
            Assert.Equal(0f, MyAccount.GetMoney(OtherName));
            Assert.Equal(0f, OtherAccount.GetMoney(OtherName));
            Assert.Equal(0f, ThirdAccount.GetMoney(MyName));
        }

        [Fact]
        public void CanExchangeArtefactsForMoney()
        {
            var money = (Money)100;
            Interconnect(MyActor, OtherActor, ThirdActor);
            SetMutualTrust((SignedWeight)1, MyActor, OtherActor, ThirdActor);
            InitializeMoney(money, MyActor, OtherActor, ThirdActor);
            var artefact1 = OtherActor.CreateArtefact(ArtefactName);
            var artefact2 = OtherActor.CreateArtefact(AnotherArtefactName);

            Assert.True(MakeTransaction(OtherActor, MyActor, money, artefact1, artefact2));

            Assert.Equal(Money.Min, ThirdAccount.GetMoney(MyName));
            Assert.Equal(2 * money, ThirdAccount.GetMoney(OtherName));
            Assert.Equal(MyName, ThirdAccount.GetArtefact(artefact1.Name).OwnerName);
            Assert.Equal(MyName, ThirdAccount.GetArtefact(artefact2.Name).OwnerName);
        }

        private bool MakeTransaction(IActor giver, IActor receiver, params Artefact[] artefacts)
            => MakeTransaction(giver, receiver, (Money)0, artefacts);

        private bool MakeTransaction(IActor seller, IActor buyer, Money money, params Artefact[] artefacts)
        {
            var transfers = GetTransfers(seller, buyer, money, artefacts).ToArray();
            var key = seller.StartTransaction(buyer.Account.Name, transfers);
            return !string.IsNullOrEmpty(key) && buyer.AcceptTransaction(key);
        }

        private IEnumerable<Transfer> GetTransfers(IActor seller, IActor buyer, Money money, params Artefact[] artefacts)
        {
            if (money > 0f) yield return new Transfer
            {
                Money = money,
                GiverName = buyer.Account.Name,
                ReceiverName = seller.Account.Name
            };
            if (artefacts.Any()) yield return new Transfer
            {
                Artefacts = artefacts,
                GiverName = seller.Account.Name,
                ReceiverName = buyer.Account.Name
            };
        }
    }
}