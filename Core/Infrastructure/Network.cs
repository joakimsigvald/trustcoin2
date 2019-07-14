using System;
using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Infrastructure
{
    public class Network : INetwork
    {
        private readonly IDictionary<string, IAccount> _accounts = new Dictionary<string, IAccount>();
        private readonly ICryptographyFactory _cryptographyFactory;
        private readonly ITransactionFactory _transactionFactory = new TransactionFactory();
        private readonly ILookupService _lookupService = new LookupService();

        public Network(ICryptographyFactory cryptographyFactory)
            => _cryptographyFactory = cryptographyFactory;

        public IAccount CreateRootAccount(string name, int number)
        {
            var account = new Account(_cryptographyFactory.CreateCryptography(), name, $"{number}");
            AddAccount(account);
            return account;
        }

        public void AddAccount(IAccount account)
        {
            _accounts[account.Name] = account;
            _lookupService.Add(account.Self);
        }

        public IAgent FindAgent(string name)
            => _lookupService.FindByName(name);

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

        public ILookupService GetLookupService() => _lookupService;
    }
}