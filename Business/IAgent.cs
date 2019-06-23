using System.Collections.Generic;

namespace Trustcoin.Business
{
    public interface IAgent
    {
        string Name { get; }
        bool IsEndorced { get; }
        ICollection<IAgent> Relations { get; }
    }
}