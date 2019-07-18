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
        protected readonly IAccount MyAccount, OtherAccount, ThirdAccount;
        protected readonly IActor MyActor, OtherActor, ThirdActor;
        protected readonly Artefact Artefact, AnotherArtefact;

        protected ITransactionFactory _transactionFactory = new TransactionFactory();

        protected TestBase(ICryptographyFactory cryptographyFactory = null)
        {
            _network = new Network(cryptographyFactory ?? new SimpleCryptographyFactory());
            MyAccount = _network.CreateRootAccount("MyAccount", 1);
            OtherAccount = _network.CreateRootAccount("OtherAccount", 2);
            ThirdAccount = _network.CreateRootAccount("ThirdAccount", 3);
            MyActor = MyAccount.GetActor(_network, _transactionFactory);
            OtherActor = OtherAccount.GetActor(_network, _transactionFactory);
            ThirdActor = ThirdAccount.GetActor(_network, _transactionFactory);
            Artefact = new Artefact((ArtefactId)"255:1", "SomeArtefact", (AgentId)"255");
            AnotherArtefact = new Artefact((ArtefactId)"255:2", "AnotherArtefact", (AgentId)"255");
        }

        protected AgentId MyId => MyAccount.Id;
        protected AgentId OtherId => OtherAccount.Id;
        protected AgentId ThirdId => ThirdAccount.Id;

        protected void Interconnect(params IActor[] actors)
        {
            var agentIds = actors.Select(actor => actor.Account.Id).ToList();
            agentIds.ForEach(id => ConnectFrom(id, actors.Where(acc => acc.Account.Id != id)));
        }

        protected void SetMutualTrust(SignedWeight trust, params IActor[] actors)
        {
            var agentIds = actors.Select(actor => actor.Account.Id).ToList();
            agentIds.ForEach(id => SetTrust(trust, id, actors.Where(acc => acc.Account.Id != id)));
        }

        protected void InitializeMoney(Money money, params IActor[] actors)
        {
            var agentIds = actors.Select(actor => actor.Account.Id).ToList();
            agentIds.ForEach(id => SetMoney(money, id, actors));
        }

        private void ConnectFrom(AgentId agentId, IEnumerable<IActor> actors)
            => actors.ToList().ForEach(actor => actor.Connect(agentId));

        private void SetTrust(SignedWeight trust, AgentId agentId, IEnumerable<IActor> actors)
            => actors.ToList().ForEach(actor => actor.Account.SetTrust(agentId, trust));

        private void SetMoney(Money money, AgentId agentId, IEnumerable<IActor> actors)
            => actors.ToList().ForEach(actor => actor.Account.SetMoney(agentId, money));
    }
}