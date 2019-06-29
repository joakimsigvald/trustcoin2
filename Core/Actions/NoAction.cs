using System;
using System.Runtime.Serialization;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class NoAction : ActionBase
    {
        public NoAction()
        {
        }

        protected NoAction(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}