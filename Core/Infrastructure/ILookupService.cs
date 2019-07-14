using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Infrastructure
{
    public interface ILookupService
    {
        void Add(IAgent agent);
        IAgent FindById(string id);
        IAgent FindByName(string v);
    }
}