using System;
using System.Text;

namespace Trustcoin.Core
{
    public class SimpleSignature : ISignature
    {
        private readonly string _encryptedData;

        public SimpleSignature(string payload, string privateKey)
        {
            var timeStamp = DateTime.UtcNow.Ticks;
            var checksum = timeStamp * payload.GetHashCode();
            var plaintext = $"{privateKey}{timeStamp}|{checksum}|{payload}";
            _encryptedData = Base64Encode(plaintext);
        }

        public bool Verify(string payload, string publicKey)
        {
            var plaintext = Base64Decode(_encryptedData);
            if (!plaintext.StartsWith(publicKey))
                return false;
            var parts = plaintext.Substring(publicKey.Length).Split('|', 3);
            return parts.Length == 3
                && parts[2] == payload
                && long.TryParse(parts[0], out var timestamp)
                && long.TryParse(parts[1], out var checksum)
                && checksum == ComputeChecksum(timestamp, payload);
        }

        private long ComputeChecksum(long timestamp, string payload)
            => timestamp * payload.GetHashCode();

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}