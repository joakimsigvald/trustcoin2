﻿using Trustcoin.Core.Actions;
using Trustcoin.Core.Infrastructure;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IClient
    {
        bool Update(AgentId subjectId, SignedAction action);
        Update RequestUpdate(AgentId[] subjectIds, ArtefactId[] artefactIds, int cascadeCount = 0);
        bool? Verify(Transaction transaction);
    }
}