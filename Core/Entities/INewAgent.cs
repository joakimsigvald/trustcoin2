using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface INewAgent : IIDentifiable<AgentId>
    {
        string Name { get; }
        byte[] PublicKey { get; set; }
        IAgent Clone();
    }
}