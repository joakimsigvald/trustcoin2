using System.Collections.Generic;

namespace Trustcoin.Core
{
    public interface IAccount
    {
        ICollection<IPeer> Peers { get; }
        string Name { get; }
        IPeer Self { get; }
        string PublicKey { get; }

        IPeer Connect(string name);
        bool IsConnectedTo(string name);
        IPeer GetPeer(string name);
        void Endorce(string name);
        float GetTrust(string name);
        float SetTrust(string name, float trust);
        float IncreaseTrust(string name, float factor);
        float ReduceTrust(string name, float factor);
        void RenewKeys();
    }
}