using System;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class NetworkTests : TestBase
    {
        private static readonly RsaCryptographyFactory _cryptographyFactory = new RsaCryptographyFactory();
        public readonly ICryptography _cryptography = _cryptographyFactory.CreateCryptography();

        public NetworkTests() : base(_cryptographyFactory) { }

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
            var otherAgent = _network.FindAgent(OtherAccountName);
            Assert.Throws<InvalidOperationException>(
                () => _network.SendAction(MyAccountName, OtherAccountName, _cryptography.Sign(new NoAction())));
        }

        [Fact]
        public void WhenUpdatedWithUnconnectedAgent_ReturnsFalse()
        {
            Assert.False(_network.SendAction(MyAccountName, OtherAccountName, _cryptography.Sign(new NoAction())));
        }
    }
}