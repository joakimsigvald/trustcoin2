namespace Trustcoin.Core
{
    public interface IClient
    {
        bool Update(IAgent sourceAgent, ISignature sourceSignature);
        bool IsConnectedTo(string name);
    }
}