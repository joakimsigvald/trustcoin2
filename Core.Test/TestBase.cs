using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Cryptography;

namespace Trustcoin.Core.Test
{
    public abstract class TestBase
    {
        protected readonly INetwork _network;
        protected readonly IAccount MyAccount;
        protected readonly IAccount OtherAccount;
        protected readonly IAccount ThirdAccount;
        protected const string MyAccountName = "MyAccount";
        protected const string OtherAccountName = "OtherAccount";
        protected const string ThirdAccountName = "ThirdAccount";

        protected TestBase(ICryptographyFactory cryptographyFactory = null)
        {
            _network = new Network(cryptographyFactory ?? new SimpleCryptographyFactory());
            MyAccount = _network.CreateAccount(MyAccountName);
            OtherAccount = _network.CreateAccount(OtherAccountName);
            ThirdAccount = _network.CreateAccount(ThirdAccountName);
        }

        protected void Interconnect(params IAccount[] accounts)
        {
            var agentNames = accounts.Select(account => account.Name).ToList();
            agentNames.ForEach(name => ConnectFrom(name, accounts.Where(acc => acc.Name != name)));
        }

        private void ConnectFrom(string agentName, IEnumerable<IAccount> accounts)
            => accounts.ToList().ForEach(acc => acc.Connect(agentName));
    }
}