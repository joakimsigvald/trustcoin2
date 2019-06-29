namespace Trustcoin.Core.Actions
{
    public class EndorceAction : BaseAction
    {
        public EndorceAction(ISignature signature, string agentName) : base(signature)
        {
            AgentName = agentName;
        }

        public string AgentName { get; }
    }
}