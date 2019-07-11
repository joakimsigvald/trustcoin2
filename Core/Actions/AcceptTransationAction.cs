using System;
using System.Runtime.Serialization;

namespace Trustcoin.Core.Entities
{

    [Serializable]
    public class AcceptTransationAction : TransationAction
    {
        public AcceptTransationAction(Transaction transaction) : base(transaction)
        {
        }

        protected AcceptTransationAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}