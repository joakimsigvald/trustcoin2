using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
{
    public interface INetwork
    {
        IAccount CreateAccount(string name);
        IAgent FindAgent(string name);
        bool SendAction(string targetName, string sourceName, SignedAction action);
        IDictionary<string, Money> RequestUpdate(string targetName, string[] subjectNames);
    }
}