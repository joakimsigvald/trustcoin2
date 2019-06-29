using Trustcoin.Core.Actions;

namespace Trustcoin.Core
{
    public interface INetwork
    {
        IAccount CreateAccount(string name);
        IAgent FindAgent(string name);
        bool SendAction(string targetName, string sourceName, ISignedAction action);
    }
}