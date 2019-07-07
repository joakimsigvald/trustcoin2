using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Exceptions;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class AccountTests : TestBase
    {
        private static readonly SimpleCryptographyFactory _cryptographyFactory = new SimpleCryptographyFactory();

        public AccountTests() : base(_cryptographyFactory) { }

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
        public void Account_Self_IsSameAsAccount()
        {
            Assert.Equal(MyAccount.Name, MyAccount.Self.Name);
        }

        [Fact]
        public void CanGetSelfByName()
        {
            Assert.NotNull(MyAccount.GetPeer(MyAccountName));
        }

        [Fact]
        public void WhenTryGetNonExistingPeer_ThrowsNotFoundPeer()
        {
            var name = "XXX";
            var ex = Assert.Throws<NotFound<IPeer>>(() => MyAccount.GetPeer(name));
            Assert.Equal(name, ex.ParamName);
        }
    }
}