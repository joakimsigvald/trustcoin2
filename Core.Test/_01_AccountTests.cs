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
        public void NetworkCanCreateRootAccounts()
        {
            const string firstName = "FirstName";
            const string secondName = "SecondName";
            Assert.Null(_network.FindAgent(firstName));
            Assert.Null(_network.FindAgent(secondName));

            _network.CreateRootAccount(firstName, 10);
            _network.CreateRootAccount(secondName, 11);
            var first = _network.FindAgent(firstName);
            var second = _network.FindAgent(secondName);
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal("10", first.Id);
            Assert.Equal("11", second.Id);
        }

        [Fact]
        public void ActorCanCreateAccounts()
        {
            const string firstName = "FirstName";
            const string secondName = "SecondName";
            Assert.Null(_network.FindAgent(firstName));
            Assert.Null(_network.FindAgent(secondName));

            MyActor.CreateAccount(firstName);
            MyActor.CreateAccount(secondName);
            var first = _network.FindAgent(firstName);
            var second = _network.FindAgent(secondName);
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(MyActor.Account.Id + ".1", first.Id);
            Assert.Equal(MyActor.Account.Id + ".2", second.Id);
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