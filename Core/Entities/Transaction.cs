namespace Trustcoin.Core.Entities
{
    public class Transaction
    {
        public string Key { get; set; }
        public IArtefact Artefact { get; set; }
        public string ReceiverName { get; internal set; }
    }
}