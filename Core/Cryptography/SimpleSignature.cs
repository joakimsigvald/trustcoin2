namespace Trustcoin.Core.Cryptography
{
    public class SimpleSignature : ISignature
    {
        public bool Verify(byte[] message, byte[] publicKey)
            => true;
    }
}