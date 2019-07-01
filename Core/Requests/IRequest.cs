namespace Trustcoin.Core.Requests
{
    public interface IRequest
    {
    }

    public class MoneyRequest
    {
        public MoneyRequest(string agentName) => AgentName = agentName;

        public string AgentName { get; set; }
    }
}
