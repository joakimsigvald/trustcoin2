using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public abstract class NewAgentAction : ActionBase<NewAgent, INewAgent>
    {
        public NewAgentAction(INewAgent agent) : base(agent)
        {
        }

        protected NewAgentAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}