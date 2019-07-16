using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface INewAgent
    {
        string Name { get; }
        byte[] PublicKey { get; set; }
        AgentId Id { get; }
        IAgent Clone();
    }
}