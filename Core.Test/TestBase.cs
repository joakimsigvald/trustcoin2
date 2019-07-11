using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Infrastructure;

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
        protected const string MyAccountName = "MyAccount";
        protected const string OtherAccountName = "OtherAccount";
        protected const string ThirdAccountName = "ThirdAccount";
        protected const string ArtefactName = "SomeArtefact";
        protected const string AnotherArtefactName = "AnotherArtefact";

        protected TestBase(ICryptographyFactory cryptographyFactory = null)
        {
            _network = new Network(cryptographyFactory ?? new SimpleCryptographyFactory());
            MyAccount = _network.CreateAccount(MyAccountName);
            OtherAccount = _network.CreateAccount(OtherAccountName);
            ThirdAccount = _network.CreateAccount(ThirdAccountName);
            MyActor = MyAccount.GetActor(_network);
            OtherActor = OtherAccount.GetActor(_network);
            ThirdActor = ThirdAccount.GetActor(_network);
        }

        protected void Interconnect(params IActor[] actors)
        {
            var agentNames = actors.Select(actor => actor.Name).ToList();
            agentNames.ForEach(name => ConnectFrom(name, actors.Where(acc => acc.Name != name)));
        }

        private void ConnectFrom(string agentName, IEnumerable<IActor> actors)
            => actors.ToList().ForEach(actor => actor.Connect(agentName));
    }
}