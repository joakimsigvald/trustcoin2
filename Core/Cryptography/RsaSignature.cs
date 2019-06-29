using System.Security.Cryptography;

namespace Trustcoin.Core.Cryptography
{
    public class RsaSignature : ISignature
    {
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
        private static readonly RSASignaturePadding SignaturePadding = RSASignaturePadding.Pss;
        private readonly byte[] _signature;

        public RsaSignature(byte[] message, RSA rsa)
        {
            _signature = rsa.SignData(Hash(message), HashAlgorithm, RSASignaturePadding.Pss);
        }

        public bool Verify(byte[] message, byte[] publicKey)
        {
            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(publicKey, out var _);
            return rsa.VerifyData(Hash(message), _signature, HashAlgorithm, SignaturePadding);
        }

        private byte[] Hash(byte[] message) => SHA256.Create().ComputeHash(message);
    }
}
