using System;
using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;

namespace Trustcoin.Core
{

    public class Network : INetwork
    {
        private readonly IDictionary<string, Account> _accounts = new Dictionary<string, Account>();
        private readonly ICryptographyFactory _cryptographyFactory;

        public Network(ICryptographyFactory cryptographyFactory)
            => _cryptographyFactory = cryptographyFactory;

        public IAccount CreateAccount(string name)
            => _accounts[name] = new Account(this, _cryptographyFactory.CreateCryptography(), name);

        public IAgent FindAgent(string name)
            => _accounts.TryGetValue(name, out var account) ? new Agent(account) : null;

        public bool SendAction(string targetName, string sourceName, ISignedAction action)
        {
            var targetClient = GetClient(targetName);
            return targetClient.Update(sourceName, action);
        }

        private IClient GetClient(string name)
            => _accounts[name];
    }
}