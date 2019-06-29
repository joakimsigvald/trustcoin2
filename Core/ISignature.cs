namespace Trustcoin.Core
{
    public interface ISignature
    {
        public bool Verify(byte[] message, byte[] publicKey);
    }
}