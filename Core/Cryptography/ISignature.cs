using System.Runtime.Serialization;

namespace Trustcoin.Core.Cryptography
{
    public interface ISignature
    {
        public bool Verify(byte[] message, byte[] publicKey);
    }
}