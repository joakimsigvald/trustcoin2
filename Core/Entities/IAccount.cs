using System.Collections.Generic;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IAccount
    {
        IEnumerable<IPeer> Peers { get; }
        string Name { get; }
        IPeer Self { get; }

        void SyncAll();

        byte[] PublicKey { get; }

        IPeer Connect(string name);
        bool IsConnectedTo(string name);
        IPeer GetPeer(string name);
        void Endorce(string name);
        SignedWeight GetTrust(string name);
        SignedWeight SetTrust(string name, SignedWeight value);
        SignedWeight IncreaseTrust(string name, Weight factor);
        SignedWeight DecreaseTrust(string name, Weight factor);
        void RenewKeys();
        Weight GetRelationWeight(string subjectName, string objectName);
        void SetRelationWeight(string subjectName, string objectName, Weight value);
        Money GetMoney(string name);
        void SetMoney(string name, Money money);
    }
}