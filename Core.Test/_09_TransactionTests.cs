using Trustcoin.Core;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class TransactionTests : TestBase
    {
        [Fact]
        public void AfterPeerTransferArtefactToMe_AllMyPeersKnowIHaveIt()
        {
            Interconnect(MyActor, OtherActor, ThirdActor);
            var artfact = OtherActor.CreateArtefact(ArtefactName);

            var transactionKey = OtherActor.StartTransaction(MyAccountName, artfact);
            MyActor.AcceptTransaction(transactionKey);

            Assert.Equal(MyAccountName, ThirdAccount.GetArtefact(ArtefactName).OwnerName);
        }

        [Fact]
        public void AfterPeerTransferArtefactToMe_AllPeersPeersKnowIHaveIt()
        {
            Interconnect(MyActor, OtherActor);
            Interconnect(OtherActor, ThirdActor);
            var artfact = OtherActor.CreateArtefact(ArtefactName);

            var transactionKey = OtherActor.StartTransaction(MyAccountName, artfact);
            MyActor.AcceptTransaction(transactionKey);

            Assert.Equal(MyAccountName, ThirdAccount.GetArtefact(ArtefactName).OwnerName);
        }
    }
}