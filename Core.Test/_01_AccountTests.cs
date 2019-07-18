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
        private static readonly AgentId FirstId = (AgentId)$"{10}";
        private static readonly AgentId SecondId = (AgentId)$"{11}";

        public AccountTests() : base(_cryptographyFactory) { }

        [Fact]
        public void CanFindAgentByName()
        {
            Assert.Equal(MyId, _network.FindAgent(MyId)?.Id);
        }

        [Fact]
        public void GivenAccountDoesntExist_WhenFindAgent_GetNull()
        {
            Assert.Null(_network.FindAgent((AgentId)"255"));
        }

        [Fact]
        public void NetworkCanCreateRootAccounts()
        {
            Assert.Null(_network.FindAgent(FirstId));
            Assert.Null(_network.FindAgent(SecondId));

            _network.CreateRootAccount(FirstName, 10);
            _network.CreateRootAccount(SecondName, 11);

            Assert.Equal(FirstId, _network.FindAgent(FirstId)?.Id);
            Assert.Equal(SecondId, _network.FindAgent(SecondId)?.Id);
        }

        [Fact]
        public void ActorCanCreateAccounts()
        {
            var id1 = MyActor.Account.Id + 1;
            var id2 = MyActor.Account.Id + 2;
            Assert.Null(_network.FindAgent(id1));
            Assert.Null(_network.FindAgent(id2));

            MyActor.CreateAccount(FirstName);
            MyActor.CreateAccount(SecondName);

            Assert.Equal(id1, _network.FindAgent(id1)?.Id);
            Assert.Equal(id2, _network.FindAgent(id2)?.Id);
        }

        [Fact]
        public void Account_Self_IsSameAsAccount()
        {
            Assert.Equal(MyAccount.Id, MyAccount.Self.Id);
            Assert.Equal(MyAccount.Name, MyAccount.Self.Name);
        }

        [Fact]
        public void CanGetSelfById()
        {
            Assert.NotNull(MyAccount.GetPeer(MyId));
        }

        [Fact]
        public void WhenTryGetNonExistingPeer_ThrowsNotFoundPeer()
        {
            var id = (AgentId)"255";
            var ex = Assert.Throws<NotFound<IPeer>>(() => MyAccount.GetPeer(id));
            Assert.Contains(id.ToString(), ex.Message);
            Assert.Equal("id", ex.ParamName);
        }
    }
}