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

            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            MakeTransaction(OtherActor, MyActor, artefact);

            Assert.Equal(MyId, MyAccount.GetArtefact(artefact.Id).OwnerId);
            Assert.Equal(MyId, OtherAccount.GetArtefact(artefact.Id).OwnerId);
            Assert.Equal(MyId, ThirdAccount.GetArtefact(artefact.Id).OwnerId);
        }

        [Fact]
        public void AfterPeerTransferArtefactToMe_AllPeersPeersKnowIHaveIt()
        {
            Interconnect(MyActor, OtherActor);
            Interconnect(OtherActor, ThirdActor);

            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            MakeTransaction(OtherActor, MyActor, artefact);

            Assert.Equal(MyId, ThirdAccount.GetArtefact(artefact.Id).OwnerId);
        }

        [Fact]
        public void WhenTryToUseSameTransactionKeyTwice_NothingHappens()
        {
            var sameKeyFactoryMock = new Mock<ITransactionFactory>();
            sameKeyFactoryMock.SetReturnsDefault("X");
            var myActor = MyAccount.GetActor(_network, sameKeyFactoryMock.Object);
            var otherActor = OtherAccount.GetActor(_network, sameKeyFactoryMock.Object);
            Interconnect(myActor, otherActor);

            var artefact = otherActor.CreateArtefact(Artefact.Name);
            MakeTransaction(otherActor, myActor, artefact);
            MakeTransaction(myActor, otherActor, MyAccount.GetArtefact(artefact.Id));

            Assert.True(MyAccount.Self.HasArtefact(artefact.Id));
            Assert.False(OtherAccount.Self.HasArtefact(artefact.Id));
        }

        [Fact]
        public void IfThreeOfFourPeersAgreeGiverHasArtefact_TransactionCanBeAccepted_ButIsNotAcceptedByDisagreeingPeer()
        {
            Interconnect(MyActor, OtherActor);
            var a1 = OtherActor.CreateArtefact(Artefact.Name);
            var a2 = ThirdActor.CreateArtefact(Artefact.Name);
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, (SignedWeight)0.5f);
            MyAccount.SetTrust(ThirdId, (SignedWeight)0.5f);

            Assert.True(MakeTransaction(OtherActor, MyActor, a1));
            Assert.Equal(ThirdId, ThirdAccount.GetArtefact(a2.Id).OwnerId);
        }

        [Fact]
        public void IfTwoOfThreePeersAgreeGiverHasArtefact_ItCannotBeAcceptedAndIsNotAccounted()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            ThirdActor.CounterfeitArtefact(artefact);
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, (SignedWeight)1);
            MyAccount.SetTrust(ThirdId, (SignedWeight)1);

            Assert.False(MakeTransaction(OtherActor, MyActor, artefact));
            Assert.False(MyAccount.Self.HasArtefact(artefact.Id));
        }

        [Fact]
        public void EvenIfSomePeersDontKnowArtefact_TransactionIsVerified()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, (SignedWeight)1);
            MyAccount.SetTrust(ThirdId, (SignedWeight)1);
        }

        [Fact]
        public void WhenTransactionIsNotAccounted_InvolvedPeersLoseTrust()
        {
            Interconnect(OtherActor, ThirdActor);
            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            MyActor.CounterfeitArtefact(artefact);
            Interconnect(MyActor, OtherActor, ThirdActor);
            var otherTrustBefore = MyAccount.GetTrust(OtherId);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdId);

            MakeTransaction(OtherActor, ThirdActor, artefact);

            var expectedOtherTrust = otherTrustBefore.Decrease(UnaccountedTransactionDistrustFactor);
            var expectedThirdTrust = thirdTrustBefore.Decrease(UnaccountedTransactionDistrustFactor);
            var actualOtherTrust = MyAccount.GetTrust(OtherId);
            var actualThirdTrust = MyAccount.GetTrust(ThirdId);
            Assert.Equal(expectedOtherTrust, actualOtherTrust);
            Assert.Equal(expectedThirdTrust, actualThirdTrust);
        }

        [Fact]
        public void WhenTransactionIsAccounted_InvolvedPeersGainTrust()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            var otherTrustBefore = MyAccount.GetTrust(OtherId);
            var thirdTrustBefore = MyAccount.GetTrust(ThirdId);

            MakeTransaction(OtherActor, ThirdActor, artefact);

            var expectedOtherTrust = otherTrustBefore.Increase(AccountedTransactionTrustFactor);
            var expectedThirdTrust = thirdTrustBefore.Increase(AccountedTransactionTrustFactor);
            var actualOtherTrust = MyAccount.GetTrust(OtherId);
            var actualThirdTrust = MyAccount.GetTrust(ThirdId);
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
            MyAccount.SetTrust(OtherId, (SignedWeight)acceptingPeerTrust);
            MyAccount.SetTrust(ThirdId, (SignedWeight)rejectingPeerTrust);
            MyAccount.SetMoney(OtherId, someMoney);
            OtherAccount.SetTrust(MyId, (SignedWeight)acceptingPeerTrust);
            OtherAccount.SetTrust(ThirdId, (SignedWeight)rejectingPeerTrust);
            OtherAccount.SetMoney(OtherId, someMoney);

            Assert.Equal(0f, MyAccount.GetMoney(MyId));
            Assert.Equal(0f, OtherAccount.GetMoney(MyId));
            Assert.Equal(0f, ThirdAccount.GetMoney(MyId));
            Assert.Equal(someMoney, MyAccount.GetMoney(OtherId));
            Assert.Equal(someMoney, OtherAccount.GetMoney(OtherId));
            Assert.Equal(0f, ThirdAccount.GetMoney(MyId));

            Assert.Equal(transactionAccepted, MakeTransaction(MyActor, OtherActor, someMoney));

            if (!transactionAccepted) return;
            Assert.Equal(someMoney, MyAccount.GetMoney(MyId));
            Assert.Equal(someMoney, OtherAccount.GetMoney(MyId));
            Assert.Equal(0f, ThirdAccount.GetMoney(MyId));
            Assert.Equal(0f, MyAccount.GetMoney(OtherId));
            Assert.Equal(0f, OtherAccount.GetMoney(OtherId));
            Assert.Equal(0f, ThirdAccount.GetMoney(MyId));
        }

        [Fact]
        public void CanExchangeArtefactsForMoney()
        {
            var money = (Money)100;
            Interconnect(MyActor, OtherActor, ThirdActor);
            SetMutualTrust((SignedWeight)1, MyActor, OtherActor, ThirdActor);
            InitializeMoney(money, MyActor, OtherActor, ThirdActor);
            var a1 = OtherActor.CreateArtefact(Artefact.Name);
            var a2 = OtherActor.CreateArtefact(AnotherArtefact.Name);

            Assert.True(MakeTransaction(OtherActor, MyActor, money, a1, a2));

            Assert.Equal(Money.Min, ThirdAccount.GetMoney(MyId));
            Assert.Equal(2 * money, ThirdAccount.GetMoney(OtherId));
            Assert.Equal(MyId, ThirdAccount.GetArtefact(a1.Id).OwnerId);
            Assert.Equal(MyId, ThirdAccount.GetArtefact(a2.Id).OwnerId);
        }

        private bool MakeTransaction(IActor giver, IActor receiver, params Artefact[] artefacts)
            => MakeTransaction(giver, receiver, (Money)0, artefacts);

        private bool MakeTransaction(IActor seller, IActor buyer, Money money, params Artefact[] artefacts)
        {
            var transfers = GetTransfers(seller, buyer, money, artefacts).ToArray();
            var key = seller.StartTransaction(buyer.Account.Id, transfers);
            return !string.IsNullOrEmpty(key) && buyer.AcceptTransaction(key);
        }

        private IEnumerable<Transfer> GetTransfers(IActor seller, IActor buyer, Money money, params Artefact[] artefacts)
        {
            if (money > 0f) yield return new Transfer
            {
                Money = money,
                GiverId = buyer.Account.Id,
                ReceiverId = seller.Account.Id
            };
            if (artefacts.Any()) yield return new Transfer
            {
                Artefacts = artefacts,
                GiverId = seller.Account.Id,
                ReceiverId = buyer.Account.Id
            };
        }
    }
}