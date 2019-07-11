namespace Trustcoin.Core.Entities
{
    public interface IActor
    {
        string Name { get; }
        IAccount Account { get; }

        IArtefact CreateArtefact(string name);
        void SyncAll();
        IPeer Connect(string name);
        void Endorce(string name);
        void RenewKeys();
        void DestroyArtefact(string artefactName);
        void EndorceArtefact(IArtefact artefact);
        string StartTransaction(string myAccountName, IArtefact artfact);
        void AcceptTransaction(string transactionKey);
        IPeer ProducePeer(string name);
    }
}