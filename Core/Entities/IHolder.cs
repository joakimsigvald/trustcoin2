using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IHolder : IIDentifiable<AgentId>
    {
        Money Money { get; set; }
    }

    public class Holder : IHolder
    {
        public AgentId Id { get; set; }
        public Money Money { get; set; }
    }
}