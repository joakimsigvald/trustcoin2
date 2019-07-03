using System;
using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
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

        public IDictionary<string, Money> RequestUpdate(string targetName, string[] subjectNames)
        {
            var targetClient = GetClient(targetName);
            return targetClient.RequestUpdate(subjectNames);
        }

        public bool SendAction(string targetName, string subjectName, ISignedAction action)
        {
            var targetClient = GetClient(targetName);
            return targetClient.Update(subjectName, action);
        }

        private IClient GetClient(string name)
            => _accounts[name];
    }
}