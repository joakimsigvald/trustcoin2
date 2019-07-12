using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Infrastructure
{
    public class Network : INetwork
    {
        private readonly IDictionary<string, Account> _accounts = new Dictionary<string, Account>();
        private readonly ICryptographyFactory _cryptographyFactory;
        private readonly ITransactionFactory _transactionFactory = new TransactionFactory();

        public Network(ICryptographyFactory cryptographyFactory)
            => _cryptographyFactory = cryptographyFactory;

        public IAccount CreateAccount(string name)
            => _accounts[name] = new Account(_cryptographyFactory.CreateCryptography(), name);

        public IAgent FindAgent(string name)
            => _accounts.TryGetValue(name, out var account) ? new Agent(account) : null;

        public Update RequestUpdate(string targetName, string[] subjectNames, string[] artefactNames)
        {
            var targetClient = _accounts[targetName].GetClient(this, _transactionFactory);
            return targetClient.RequestUpdate(subjectNames, artefactNames);
        }

        public bool? RequestVerification(string targetName, Transaction transaction)
        {
            var targetClient = _accounts[targetName].GetClient(this, _transactionFactory);
            return targetClient.Verify(transaction);
        }

        public bool SendAction(string targetName, string subjectName, SignedAction action)
        {
            var targetClient = _accounts[targetName].GetClient(this, _transactionFactory);
            return targetClient.Update(subjectName, action.Clone());
        }
    }
}