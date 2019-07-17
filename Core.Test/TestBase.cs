using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Test
{
    public abstract class TestBase
    {
        protected readonly INetwork _network;
        protected readonly IAccount MyAccount;
        protected readonly IAccount OtherAccount;
        protected readonly IAccount ThirdAccount;
        protected readonly IActor MyActor;
        protected readonly IActor OtherActor;
        protected readonly IActor ThirdActor;
        protected const string MyName = "MyAccount";
        protected const string OtherName = "OtherAccount";
        protected const string ThirdName = "ThirdAccount";
        protected const string ArtefactName = "SomeArtefact";
        protected const string AnotherArtefactName = "AnotherArtefact";

        protected ITransactionFactory _transactionFactory = new TransactionFactory();

        protected TestBase(ICryptographyFactory cryptographyFactory = null)
        {
            _network = new Network(cryptographyFactory ?? new SimpleCryptographyFactory());
            MyAccount = _network.CreateRootAccount(MyName, 1);
            OtherAccount = _network.CreateRootAccount(OtherName, 2);
            ThirdAccount = _network.CreateRootAccount(ThirdName, 3);
            MyActor = MyAccount.GetActor(_network, _transactionFactory);
            OtherActor = OtherAccount.GetActor(_network, _transactionFactory);
            ThirdActor = ThirdAccount.GetActor(_network, _transactionFactory);
        }

        protected void Interconnect(params IActor[] actors)
        {
            var agentNames = actors.Select(actor => actor.Account.Name).ToList();
            agentNames.ForEach(name => ConnectFrom(name, actors.Where(acc => acc.Account.Name != name)));
        }

        protected void SetMutualTrust(SignedWeight trust, params IActor[] actors)
        {
            var agentNames = actors.Select(actor => actor.Account.Name).ToList();
            agentNames.ForEach(name => SetTrust(trust, name, actors.Where(acc => acc.Account.Name != name)));
        }

        protected void InitializeMoney(Money money, params IActor[] actors)
        {
            var agentNames = actors.Select(actor => actor.Account.Name).ToList();
            agentNames.ForEach(name => SetMoney(money, name, actors));
        }

        private void ConnectFrom(string agentName, IEnumerable<IActor> actors)
            => actors.ToList().ForEach(actor => actor.Connect(agentName));

        private void SetTrust(SignedWeight trust, string agentName, IEnumerable<IActor> actors)
            => actors.ToList().ForEach(actor => actor.Account.SetTrust(agentName, trust));

        private void SetMoney(Money money, string agentName, IEnumerable<IActor> actors)
            => actors.ToList().ForEach(actor => actor.Account.SetMoney(agentName, money));
    }
}