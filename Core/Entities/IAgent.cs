using System.Collections.Generic;

namespace Trustcoin.Core.Entities
{

    public interface IAgent : INewAgent
    {
        ICollection<Relation> Relations { get; }
        bool IsConnectedTo(string targetName);
        Relation GetRelation(string agentName);
        Relation AddRelation(IAgent agent);
        IPeer AsPeer();
    }
}