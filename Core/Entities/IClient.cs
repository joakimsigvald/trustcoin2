using Trustcoin.Core.Actions;
using Trustcoin.Core.Infrastructure;

namespace Trustcoin.Core.Entities
{
    public interface IClient
    {
        bool Update(string subjectName, SignedAction action);
        Update RequestUpdate(string[] subjectNames, string[] artefactNames);
        bool? Verify(Transaction transaction);
    }
}