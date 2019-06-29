using System;
using System.Runtime.Serialization;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class EndorceAction : ActionBase
    {
        public EndorceAction(string agentName)
        {
            AgentName = agentName;
        }

        protected EndorceAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            AgentName = (string)info.GetValue(nameof(AgentName), typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(AgentName), AgentName);
        }

        public string AgentName { get; }
    }
}