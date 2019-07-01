using Trustcoin.Core.Cryptography;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class NetworkTests : TestBase
    {
        private static readonly SimpleCryptographyFactory _cryptographyFactory = new SimpleCryptographyFactory();

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
    }
}