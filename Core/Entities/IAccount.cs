using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IAccount
    {
        IEnumerable<IPeer> Peers { get; }
        ICollection<Artefact> Artefacts { get; }
        string Name { get; }
        IPeer Self { get; }
        byte[] PublicKey { get; }
        AgentId Id { get; }

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
        Artefact GetArtefact(string name);
        void ForgetArtefact(string name);
        void VerifySignature(SignedAction signedAction, IPeer peer);
        bool KnowsArtefact(string name);
        void RememberArtefact(Artefact artefact);
        IAccount CreateChild(string name);
        void RemoveArtefact(Artefact artefact);
        void AddArtefact(string artefactName, string ownerName);
        void MoveArtefact(Artefact artefact, string ownerName);
        SignedAction Sign(IAction action);
        IClient GetClient(INetwork network, ITransactionFactory transactionFactory);
        IActor GetActor(INetwork network, ITransactionFactory transactionFactory);
        Artefact ProduceArtefact(string name);

        Transaction GetPendingTransaction(string key);
        void AddTransaction(Transaction action);
        bool HasPendingTransaction(string key);
        void ClosePendingTransaction(string key);
        bool HasReceivedTransaction(string key);
        void AddPendingTransaction(Transaction transaction);
        void AddReceivedTransaction(string key);
        void IncreaseMoney(string name, Money money);
        void DecreaseMoney(string name, Money money);
    }
}