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
        Weight GetTrust(string name);
        Weight SetTrust(string name, Weight trust);
        Weight IncreaseTrust(string name, Weight factor);
        Weight ReduceTrust(string name, Weight factor);
        void RenewKeys();
    }
}