using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class ConnectAction : AgentIdAction
    {
        protected ConnectAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ConnectAction(AgentId agentId) : base(agentId)
        {
        }
    }
}