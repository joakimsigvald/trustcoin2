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
        public void IfSellerDontBelieveBuyerHasMoney_TransactionIsNotStarted()
        {
            Interconnect(MyActor, OtherActor);
            Assert.Null(StartTransaction(MyActor, OtherActor, SomeMoney));
        }

        [Fact]
        public void IfBuyerDontBelieveSellerHasArtefact_TransactionIsNotAccepted()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = MyActor.CreateArtefact(Artefact.Name);
            OtherActor.CounterfeitArtefact(artefact);
            var key = StartTransaction(MyActor, OtherActor, artefact);
            Assert.NotNull(key);
            Assert.NotEmpty(key);
            Assert.False(OtherActor.AcceptTransaction(key));
        }

        [Fact]
        public void IfPeerDontAgreeGiverHasArtefact_TransactionIsNotAcceptedByDisagreeingPeer()
        {
            Interconnect(MyActor, OtherActor);
            var artefact = OtherActor.CreateArtefact(Artefact.Name);
            ThirdActor.CounterfeitArtefact(artefact);
            Interconnect(MyActor, OtherActor, ThirdActor);
            MyAccount.SetTrust(OtherId, (SignedWeight)1);
            MyAccount.SetTrust(ThirdId, (SignedWeight)1);

            Assert.True(MakeTransaction(OtherActor, MyActor, artefact));
            Assert.True(ThirdAccount.Self.HasArtefact(artefact.Id));
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
            var key = StartTransaction(seller, buyer, money, artefacts);
            return !string.IsNullOrEmpty(key) && buyer.AcceptTransaction(key);
        }

        private string StartTransaction(IActor giver, IActor receiver, params Artefact[] artefacts)
            => StartTransaction(giver, receiver, (Money)0, artefacts);

        private string StartTransaction(IActor seller, IActor buyer, Money money, params Artefact[] artefacts)
        {
            var transfers = GetTransfers(seller, buyer, money, artefacts).ToArray();
            return seller.StartTransaction(buyer.Account.Id, transfers);
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