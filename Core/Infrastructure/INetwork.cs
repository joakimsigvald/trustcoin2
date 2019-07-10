﻿using Trustcoin.Core.Actions;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Infrastructure
{
    public interface INetwork
    {
        IAccount CreateAccount(string name);
        IAgent FindAgent(string name);
        bool SendAction(string targetName, string sourceName, SignedAction action);
        Update RequestUpdate(string targetName, string[] subjectNames, string[] artefactNames);
    }
}