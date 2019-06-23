using System.Collections.Generic;
using Trustcoin.Business;
using Xunit;

namespace Business.Test
{
    public class PeerTests : NetworkTestBase
    {
        [Fact]
        public void Account_Me_IsITsOwnPeer()
        {
            Assert.IsAssignableFrom<IPeer>(MyAccount.Me);
            Assert.Equal(MyAccount.Name, MyAccount.Me.Name);
        }

        [Fact]
        public void WhenConnectAgent_ItBecomesPeer()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.NotNull(MyAccount.GetPeer(OtherAccountName));
        }

        [Fact]
        public void WhenTryGetNonExistingPeer_ThrowsNotFoundPeer()
        {
            var name = "XXX";
            var ex = Assert.Throws<NotFound<Peer>>(() => MyAccount.GetPeer(name));
            Assert.Equal(name, ex.ParamName);
        }
    }
}