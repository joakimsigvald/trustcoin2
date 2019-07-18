using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
{
    public interface ILookupService
    {
        void Add(IAgent agent);
        IAgent Find(AgentId id);
    }
}