using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class CreateChildAction : AgentAction
    {
        public CreateChildAction(IAgent child) : base(child)
        {
        }

        protected CreateChildAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}