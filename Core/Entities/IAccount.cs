﻿using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IAccount
    {
        IEnumerable<IPeer> Peers { get; }
        string Name { get; }
        IPeer Self { get; }
        byte[] PublicKey { get; }
        bool IsConnectedTo(string name);
        IPeer GetPeer(string name);
        SignedWeight GetTrust(string name);
        SignedWeight SetTrust(string name, SignedWeight value);
        SignedWeight IncreaseTrust(string name, Weight factor);
        SignedWeight DecreaseTrust(string name, Weight factor);
        void RenewKeys();
        Weight GetRelationWeight(string subjectName, string objectName);
        void SetRelationWeight(string subjectName, string objectName, Weight value);
        void AddPeer(IPeer newPeer);
        Money GetMoney(string name);
        void SetMoney(string name, Money money);
        IArtefact GetArtefact(string name);
        void ForgetArtefact(string name);
        void VerifySignature(SignedAction signedAction, IPeer peer);
        bool KnowsArtefact(string name);
        void RememberArtefact(IArtefact artefact);
        SignedAction Sign(IAction action);
        IClient GetClient(INetwork network);
        IActor GetActor(INetwork network);
    }
}