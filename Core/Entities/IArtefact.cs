namespace Trustcoin.Core.Entities
{
    public interface IArtefact
    {
        string Name { get; }

        void AddEndorcer(IAgent peer);
        void RemoveEndorcer(IAgent peer);
        bool IsEndorcedBy(string agentName);
        string OwnerName { get; }
    }
}