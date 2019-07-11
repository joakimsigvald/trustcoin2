using System;
using System.Runtime.Serialization;

namespace Trustcoin.Core.Entities
{
    [Serializable]
    public class StartTransationAction : TransationAction
    {
        public StartTransationAction(Transaction transaction) : base(transaction)
        {
        }

        protected StartTransationAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}