namespace Trustcoin.Core
{
    public interface ISignature
    {
        public bool Verify(string payload, string publicKey);
    }
}