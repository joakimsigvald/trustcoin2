using System.Runtime.Serialization;

namespace Trustcoin.Core.Actions
{
    public interface IAction : ISerializable
    {
        byte[] Serialize();
        public IAction Clone();
    }
}