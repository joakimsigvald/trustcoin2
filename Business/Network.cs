using System.Collections.Generic;

namespace Trustcoin.Business
{

    public class Network : INetwork
    {
        private readonly IDictionary<string, Account> _accounts = new Dictionary<string, Account>();

        public IAccount CreateAccount(string name)
            => _accounts[name] = new Account(this, name);

        public IAgent FindAgent(string name)
            => _accounts.TryGetValue(name, out var account) ? new Agent(account) : null;
    }
}