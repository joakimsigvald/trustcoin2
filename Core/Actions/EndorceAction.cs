﻿using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class EndorceAction : AgentIdAction
    {
        protected EndorceAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public EndorceAction(AgentId agentId) : base(agentId)
        {
        }
    }
}