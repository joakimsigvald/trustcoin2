﻿using System.Collections.Generic;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IAccount
    {
        IEnumerable<IPeer> Peers { get; }
        string Name { get; }
        IPeer Self { get; }
        byte[] PublicKey { get; }

        IPeer Connect(string name);
        bool IsConnectedTo(string name);
        IPeer GetPeer(string name);
        void Endorce(string name);
        Weight GetTrust(string name);
        Weight SetTrust(string name, Weight value);
        Weight IncreaseTrust(string name, Weight factor);
        Weight ReduceTrust(string name, Weight factor);
        void RenewKeys();
        Weight GetRelationWeight(string subjectName, string objectName);
        void SetRelationWeight(string subjectName, string objectName, Weight value);
        Money GetMoney(string name);
        void SetMoney(string name, Money money);
    }
}