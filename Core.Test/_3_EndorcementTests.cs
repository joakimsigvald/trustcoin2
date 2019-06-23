using Xunit;

namespace Core.Test
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
    }
}