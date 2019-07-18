using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public abstract class AgentIdAction : ActionBase<AgentId, AgentId>
    {
        protected AgentIdAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public AgentIdAction(AgentId agentId) : base(agentId)
        {
        }
    }
}