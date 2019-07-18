using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IActor
    {
        IAccount Account { get; }
        Artefact CreateArtefact(string name);
        void CounterfeitArtefact(Artefact artefact);
        void SyncAll();
        IPeer Connect(AgentId id);
        void Endorce(AgentId id);
        void RenewKeys();
        void DestroyArtefact(ArtefactId artefactId);
        void EndorceArtefact(Artefact artefact);
        string StartTransaction(AgentId clientId, params Transfer[] transfers);
        string StartTransaction(AgentId clientId, Money money);
        bool AcceptTransaction(string transactionKey);
        IPeer ProducePeer(AgentId id);
        void RelayTransactionAccepted(Transaction transaction);
        IAccount CreateAccount(string name);
    }
}