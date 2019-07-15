using System.Collections.Generic;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IAgent
    {
        string Name { get; }
        byte[] PublicKey { get; set; }
        ICollection<Relation> Relations { get; }
        AgentId Id { get; }

        bool IsConnectedTo(string targetName);
        IAgent Clone();
        Relation GetRelation(string agentName);
        Relation AddRelation(IAgent agent);
        IPeer AsPeer();
    }
}