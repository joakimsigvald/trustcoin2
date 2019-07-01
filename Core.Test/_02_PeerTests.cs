using Trustcoin.Core.Entities;
using Trustcoin.Core.Exceptions;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class PeerTests : TestBase
    {
        [Fact]
        public void Account_Me_IsITsOwnPeer()
        {
            Assert.IsAssignableFrom<IPeer>(MyAccount.Self);
            Assert.Equal(MyAccount.Name, MyAccount.Self.Name);
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