﻿using System.Collections.Generic;

namespace Trustcoin.Core
{
    public interface IAgent
    {
        string Name { get; }
        string PublicKey { get; set; }
        bool IsEndorced { get; set; }
        ICollection<Relation> Relations { get; }
        bool IsConnectedTo(string targetName);
        IAgent Clone();
        Relation GetRelation(string agentName);
        Relation AddRelation(IAgent agent);
    }
}