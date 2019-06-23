using System.Collections.Generic;

namespace Trustcoin.Core
{
    public interface IAgent
    {
        string Name { get; }
        string PublicKey { get; }
        bool IsEndorced { get; }
        ICollection<IAgent> Relations { get; }
        bool IsConnectedTo(string targetName);
        IAgent Clone();
    }
}