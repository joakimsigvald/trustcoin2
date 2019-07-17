using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test
{
    public class AccountTests : TestBase
    {
        private static readonly SimpleCryptographyFactory _cryptographyFactory = new SimpleCryptographyFactory();
        private const string FirstName = "FirstName";
        private const string SecondName = "SecondName";

        public AccountTests() : base(_cryptographyFactory) { }

        [Fact]
        public void CanFindAgentByName()
        {
            Assert.Equal(MyName, _network.FindAgent(MyName)?.Name);
        }

        [Fact]
        public void GivenAccountDoesntExist_WhenFindAgent_GetNull()
        {
            Assert.Null(_network.FindAgent("XXX"));
        }

        [Fact]
        public void NetworkCanCreateRootAccounts()
        {
            Assert.Null(_network.FindAgent(FirstName));
            Assert.Null(_network.FindAgent(SecondName));

            _network.CreateRootAccount(FirstName, 10);
            _network.CreateRootAccount(SecondName, 11);

            Assert.Equal((AgentId)"10", _network.FindAgent(FirstName)?.Id);
            Assert.Equal((AgentId)"11", _network.FindAgent(SecondName)?.Id);
        }

        [Fact]
        public void ActorCanCreateAccounts()
        {
            Assert.Null(_network.FindAgent(FirstName));
            Assert.Null(_network.FindAgent(SecondName));

            MyActor.CreateAccount(FirstName);
            MyActor.CreateAccount(SecondName);

            Assert.Equal(MyActor.Account.Id + 1, _network.FindAgent(FirstName)?.Id);
            Assert.Equal(MyActor.Account.Id + 2, _network.FindAgent(SecondName)?.Id);
        }

        [Fact]
        public void Account_Self_IsSameAsAccount()
        {
            Assert.Equal(MyAccount.Id, MyAccount.Self.Id);
            Assert.Equal(MyAccount.Name, MyAccount.Self.Name);
        }

        [Fact]
        public void CanGetSelfByName()
        {
            Assert.NotNull(MyAccount.GetPeer(MyName));
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