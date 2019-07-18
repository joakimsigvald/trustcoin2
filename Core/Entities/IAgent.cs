using System.Collections.Generic;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{

    public interface IAgent : INewAgent
    {
        ICollection<Relation> Relations { get; }
        bool IsConnectedTo(AgentId targetId);
        Relation GetRelation(AgentId agentId);
        Relation AddRelation(IAgent agent);
        IPeer AsPeer();
    }
}