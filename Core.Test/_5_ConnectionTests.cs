using Xunit;

namespace Core.Test
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
        public void AfterConnectWithPeer_UpdateIsSentToPeers()
        {
            MyAccount.Connect(OtherAccountName);
            Assert.False(MyAccount.GetPeer(OtherAccountName).IsConnectedTo(MyAccountName));
            OtherAccount.Connect(MyAccountName);
            Assert.True(MyAccount.GetPeer(OtherAccountName).IsConnectedTo(MyAccountName));
        }

        [Fact]
        public void AfterConnectWithAgent_UpdateIsSentToPeers()
        {
            MyAccount.Connect(OtherAccountName);
            OtherAccount.Connect(MyAccountName);

            MyAccount.Connect(ThirdAccountName);

            Assert.True(OtherAccount.GetPeer(MyAccountName).IsConnectedTo(ThirdAccountName));
        }

        [Fact]
        public void AfterEndorcedPeer_UpdateIsSentToPeers()
        {
            MyAccount.Connect(OtherAccountName);
            OtherAccount.Connect(MyAccountName);

            OtherAccount.Endorce(MyAccountName);

            Assert.True(MyAccount.GetPeer(OtherAccountName).Endorces(MyAccountName));
        }
    }
}