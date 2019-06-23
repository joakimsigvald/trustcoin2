namespace Trustcoin.Core
{
    public class FakeSignature : ISignature
    {
        public FakeSignature(string payload)
        {
            Payload = payload;
        }

        public string Payload { get; }

        public bool Verify(string payload, string publicKey)
            => Payload == payload;
    }
}