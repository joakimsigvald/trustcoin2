using System;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class ConnectionTests : TestBase
    {
        [Fact]
        public void AfterIConnectWithAgent_IAmConnectedToAgent()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.True(MyAccount.IsConnectedTo(OtherAccountName));
        }

        [Fact]
        public void AfterIConnectWithAgent_AgentIsNotConnectedToMe()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.False(OtherAccount.IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void AfterMyPeerConnectWithMe_IAmUpdatedOfPeerConnection()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.False(MyAccount.GetPeer(OtherAccountName).IsConnectedTo(MyAccountName));
            OtherAccount.Connect(MyAccountName);
            Assert.True(MyAccount.GetPeer(OtherAccountName).IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void AfterMyPeerConnectWithOtherAgent_IAmUpdateOfPeerConnection()
        {
            MyAccount.Connect(OtherAccountName);
            OtherAccount.Connect(MyAccountName);

            MyAccount.Connect(ThirdAccountName);

            Assert.True(OtherAccount.GetPeer(MyAccountName).IsConnectedTo(ThirdAccountName));
        }

        [Fact]
        public void AfterPeerEndorceMe_IAmUpdatedOfEndorcement()
        {
            MyAccount.Connect(OtherAccountName);
            OtherAccount.Connect(MyAccountName);

            OtherAccount.Endorce(MyAccountName);

            Assert.True(MyAccount.GetPeer(OtherAccountName).Endorces(MyAccountName));
        }

        [Fact]
        public void WhenConnectWithSelf_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => MyAccount.Connect(MyAccountName));
        }
    }
}