using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Cryptography;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
{
    public class Network : INetwork
    {
        private readonly IDictionary<AgentId, IAccount> _accounts = new Dictionary<AgentId, IAccount>();
        private readonly ICryptographyFactory _cryptographyFactory;
        private readonly ITransactionFactory _transactionFactory = new TransactionFactory();
        private readonly ILookupService _lookupService = new LookupService();

        public Network(ICryptographyFactory cryptographyFactory)
            => _cryptographyFactory = cryptographyFactory;

        public IAccount CreateRootAccount(string name, byte number)
        {
            var account = new Account(_cryptographyFactory.CreateCryptography(), name, number);
            AddAccount(account);
            return account;
        }

        public IActor CreateActor(IAccount account)
            => new Actor(this, account, _transactionFactory);

        public void AddAccount(IAccount account)
        {
            _accounts[account.Id] = account;
            _lookupService.Add(account.Self);
        }

        public IAgent FindAgent(AgentId id)
            => _lookupService.Find(id);

        public Update RequestUpdate(AgentId targetId, AgentId[] subjectIds, ArtefactId[] artefactIds, int cascadeCount = 0)
        {
            var targetClient = _accounts[targetId].GetClient(this, _transactionFactory);
            return targetClient.RequestUpdate(subjectIds, artefactIds, cascadeCount);
        }

        public bool? RequestVerification(AgentId targetId, Transaction transaction)
        {
            var targetClient = _accounts[targetId].GetClient(this, _transactionFactory);
            return targetClient.Verify(transaction);
        }

        public bool SendAction(AgentId targetId, AgentId subjectId, SignedAction action)
        {
            var targetClient = _accounts[targetId].GetClient(this, _transactionFactory);
            return targetClient.Update(subjectId, action.Clone());
        }

        public ILookupService GetLookupService() => _lookupService;
    }
}