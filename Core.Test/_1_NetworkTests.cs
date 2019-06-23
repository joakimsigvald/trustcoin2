using System;
using Trustcoin.Core;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class NetworkTests : TestBase
    {
        [Fact]
        public void CanFindAgentByName()
        {
            var agent = _network.FindAgent(MyAccountName);
            Assert.NotNull(agent);
            Assert.Equal(MyAccountName, agent.Name);
        }

        [Fact]
        public void GivenAccountDoesntExist_WhenFindAgent_GetNull()
        {
            Assert.Null(_network.FindAgent("XXX"));
        }

        [Fact]
        public void CanCreateAccount()
        {
            const string accountName = "SomeName";
            Assert.Null(_network.FindAgent(accountName));
            _network.CreateAccount(accountName);
            Assert.NotNull(_network.FindAgent(accountName));
        }

        [Fact]
        public void WhenUpdatedWithInvalidSignature_ThrowsInvalidOperationException()
        {
            MyAccount.Connect(OtherAccountName);
            var invalidSignature = new FakeSignature("XXX");
            var otherAgent = _network.FindAgent(OtherAccountName);
            Assert.Throws<InvalidOperationException>(() => _network.Update(MyAccountName, OtherAccountName, invalidSignature));
        }

        [Fact]
        public void WhenUpdatedWithUnconnectedAgent_ThrowsInvalidOperationException()
        {
            var invalidSignature = new FakeSignature("XXX");
            var otherAgent = _network.FindAgent(OtherAccountName);
            var signature = new FakeSignature(OtherAccountName);
            Assert.Throws<InvalidOperationException>(() => _network.Update(MyAccountName, OtherAccountName, signature));
        }
    }
}