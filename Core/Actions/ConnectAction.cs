namespace Trustcoin.Core.Actions
{
    public class ConnectAction : BaseAction
    {
        public ConnectAction(ISignature signature, string agentName) : base(signature)
        {
            AgentName = agentName;
        }

        public string AgentName { get; }
    }
}