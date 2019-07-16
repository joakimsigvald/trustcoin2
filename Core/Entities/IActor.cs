using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IActor
    {
        IAccount Account { get; }
        Artefact CreateArtefact(string name);
        void SyncAll();
        IPeer Connect(string name);
        void Endorce(string name);
        void RenewKeys();
        void DestroyArtefact(string artefactName);
        void EndorceArtefact(Artefact artefact);
        string StartTransaction(string clientName, Artefact artfact);
        string StartTransaction(string clientName, Money money);
        bool AcceptTransaction(string transactionKey);
        IPeer ProducePeer(string name);
        void RelayTransactionAccepted(Transaction transaction);
        IAccount CreateAccount(string firstName);
    }
}