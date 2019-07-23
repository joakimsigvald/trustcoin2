using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IActor
    {
        IAccount Account { get; }
        Artefact CreateArtefact(string name, bool isResilient = false);
        void CounterfeitArtefact(Artefact artefact);
        void SyncAll();
        IPeer Connect(AgentId id);
        void Endorce(AgentId id);
        void RenewKeys();
        void DestroyArtefact(ArtefactId artefactId);
        void EndorceArtefact(Artefact artefact);
        string StartTransaction(AgentId clientId, params Transfer[] transfers);
        bool AcceptTransaction(string transactionKey);
        bool VerifyTransaction(Transaction transaction);
        IPeer ProducePeer(AgentId id);
        void RelayTransactionAccepted(Transaction transaction);
        IAccount CreateAccount(string name);
        IHolder GetPeerAssessment(AgentId id, params AgentId[] asking);
    }
}