using Trustcoin.Core.Entities;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class PeerTests : TestBase
    {
        [Fact]
        public void Account_Self_IsSameAsAccount()
        {
            Assert.Equal(MyAccount.Name, MyAccount.Self.Name);
        }

        [Fact]
        public void Account_Self_IsEndorced()
        {
            Assert.True(MyAccount.Self.IsEndorced);
        }

        [Fact]
        public void Account_TrustForSelfIsMax()
        {
            Assert.Equal(Weight.Max, MyAccount.Self.Trust);
        }

        [Fact]
        public void IsConnectedToSelf()
        {
            Assert.True(MyAccount.IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void CanGetSelfByName()
        {
            Assert.NotNull(MyAccount.GetPeer(MyAccountName));
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