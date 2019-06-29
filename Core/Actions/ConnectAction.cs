using System;
using System.Runtime.Serialization;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class ConnectAction : ActionBase
    {
        protected ConnectAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            AgentName = (string)info.GetValue(nameof(AgentName), typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(AgentName), AgentName);
        }

        public ConnectAction(string agentName)
        {
            AgentName = agentName;
        }

        public string AgentName { get; }
    }
}