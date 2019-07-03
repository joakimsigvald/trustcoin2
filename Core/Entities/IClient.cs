using System.Collections.Generic;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IClient
    {
        bool Update(string subjectName, ISignedAction action);
        bool IsConnectedTo(string name);
        IDictionary<string, Money> RequestUpdate(string[] subjectNames);
    }
}