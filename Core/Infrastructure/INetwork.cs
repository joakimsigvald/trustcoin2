using Trustcoin.Core.Actions;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Infrastructure
{
    public interface INetwork
    {
        IAccount CreateRootAccount(string name, int number);
        void AddAccount(IAccount account);
        IAgent FindAgent(string name);
        ILookupService GetLookupService();
        bool SendAction(string targetName, string sourceName, SignedAction action);
        Update RequestUpdate(string targetName, string[] subjectNames, string[] artefactNames);
        bool? RequestVerification(string targetName, Transaction transaction);
    }
}