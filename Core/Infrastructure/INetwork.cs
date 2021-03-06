﻿using Trustcoin.Core.Actions;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
{
    public interface INetwork
    {
        IAccount CreateRootAccount(string name, byte number);
        IActor CreateActor(IAccount account);
        void AddAccount(IAccount account);
        IAgent FindAgent(AgentId id);
        ILookupService GetLookupService();
        bool SendAction(AgentId targetId, AgentId sourceId, SignedAction action);
        Update RequestUpdate(AgentId asked, AgentId[] about, ArtefactId[] regarding, params AgentId[] asking);
    }
}