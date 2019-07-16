using System.Runtime.Serialization;
using Trustcoin.Core.Actions;

namespace Trustcoin.Core.Entities
{
    public abstract class TransationAction : ActionBase<Transaction, Transaction>
    {
        protected TransationAction(Transaction transaction) : base(transaction)
        {
        }

        protected TransationAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}