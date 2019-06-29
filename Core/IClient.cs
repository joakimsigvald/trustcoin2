using Trustcoin.Core.Actions;

namespace Trustcoin.Core
{
    public interface IClient
    {
        bool Update(string sourceAgentName, IAction action);
        bool IsConnectedTo(string name);
    }
}