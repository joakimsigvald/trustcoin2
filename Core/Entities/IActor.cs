namespace Trustcoin.Core.Entities
{
    public interface IActor
    {
        IAccount Account { get; }
        IArtefact CreateArtefact(string name);
        void SyncAll();
        IPeer Connect(string name);
        void Endorce(string name);
        void RenewKeys();
        void DestroyArtefact(string artefactName);
        void EndorceArtefact(IArtefact artefact);
        string StartTransaction(string clientName, IArtefact artfact);
        bool AcceptTransaction(string transactionKey);
        IPeer ProducePeer(string name);
        void RelayTransactionAccepted(Transaction transaction);
        IAccount CreateAccount(string firstName);
    }
}