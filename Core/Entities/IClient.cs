using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IClient
    {
        bool Update(string subjectName, SignedAction action);
        IDictionary<string, Money> RequestUpdate(string[] subjectNames);
    }
}