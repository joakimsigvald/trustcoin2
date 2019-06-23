namespace Trustcoin.Core
{
    public interface INetwork
    {
        IAccount CreateAccount(string name);
        IAgent FindAgent(string name);
        bool Update(string targetName, string sourceName, ISignature sourceSignature);
    }
}