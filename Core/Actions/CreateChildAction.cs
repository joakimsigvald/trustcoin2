using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class CreateChildAction : NewAgentAction
    {
        public CreateChildAction(INewAgent child) : base(child)
        {
        }

        protected CreateChildAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}