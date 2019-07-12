using System;

namespace Trustcoin.Core.Entities
{
    public interface ITransactionFactory
    {
        string CreateTransactionKey();
    }

    public class TransactionFactory : ITransactionFactory
    {
        public string CreateTransactionKey() => Guid.NewGuid().ToString();
    }
}