using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public abstract class AgentAction : ActionBase
    {
        public AgentAction(IAgent agent)
        {
            Agent = agent;
        }

        protected AgentAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var name = info.GetString(nameof(IAgent.Name));
            var id = (AgentId)info.GetString(nameof(IAgent.Id));
            var publicKey = (byte[])info.GetValue(nameof(IAgent.PublicKey), typeof(byte[]));
            Agent = new Agent(name, id, publicKey);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(IAgent.Name), Agent.Name);
            info.AddValue(nameof(IAgent.Id), Agent.Id.ToString());
            info.AddValue(nameof(IAgent.PublicKey), Agent.PublicKey);
        }

        public IAgent Agent { get; }
    }
}