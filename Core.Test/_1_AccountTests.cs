using System;
using Trustcoin.Core;
using Xunit;

namespace Core.Test
{
    public class NetworkTests : TestBase
    {
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
            Assert.Throws<InvalidOperationException>(() => MyAccount.Update(otherAgent, invalidSignature));
        }

        [Fact]
        public void WhenUpdatedWithUnconnectedAgent_ThrowsInvalidOperationException()
        {
            var invalidSignature = new FakeSignature("XXX");
            var otherAgent = _network.FindAgent(OtherAccountName);
            var signature = new FakeSignature(OtherAccountName);
            Assert.Throws<InvalidOperationException>(() => MyAccount.Update(otherAgent, signature));
        }
    }
}