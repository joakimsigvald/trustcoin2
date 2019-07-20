using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IAccount
    {
        IEnumerable<IPeer> Peers { get; }
        IEnumerable<IPeer> OtherPeers { get; }
        ICollection<Artefact> Artefacts { get; }
        string Name { get; }
        IPeer Self { get; }
        byte[] PublicKey { get; }
        AgentId Id { get; }

        bool IsConnectedTo(AgentId agentId);
        IPeer GetPeer(AgentId id);
        SignedWeight GetTrust(AgentId agentId);
        SignedWeight SetTrust(AgentId agentId, SignedWeight value);
        SignedWeight IncreaseTrust(AgentId agentId, Weight factor);
        SignedWeight DecreaseTrust(AgentId agentId, Weight factor);
        void RenewKeys();
        Weight GetRelationWeight(AgentId subjectId, AgentId objectId);
        void SetRelationWeight(AgentId subjectId, AgentId objectId, Weight value);
        void AddPeer(IPeer newPeer);
        Money GetMoney(AgentId id);
        void SetMoney(AgentId id, Money money);
        Artefact GetArtefact(ArtefactId id);
        void ForgetArtefact(ArtefactId id);
        void VerifySignature(SignedAction signedAction, IPeer peer);
        bool KnowsArtefact(ArtefactId id);
        void RememberArtefact(Artefact artefact);
        IAccount CreateChild(string name);
        void RemoveArtefact(Artefact artefact);
        void AddArtefact(Artefact artefact, AgentId ownerId);
        void MoveArtefact(Artefact artefact, AgentId ownerId);
        SignedAction Sign(IAction action);
        IClient GetClient(INetwork network, ITransactionFactory transactionFactory);
        IActor GetActor(INetwork network, ITransactionFactory transactionFactory);
        Artefact ProduceArtefact(Artefact artefact);

        Transaction GetPendingTransaction(string key);
        void AddTransaction(Transaction action);
        bool HasPendingTransaction(string key);
        void ClosePendingTransaction(string key);
        bool HasReceivedTransaction(string key);
        void AddPendingTransaction(Transaction transaction);
        void AddReceivedTransaction(string key);
        void IncreaseMoney(AgentId id, Money money);
        void DecreaseMoney(AgentId id, Money money);
        Artefact CreateArtefact(string name, bool isResilient);
    }
}