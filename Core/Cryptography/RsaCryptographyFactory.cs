namespace Trustcoin.Core.Cryptography
{
    public class RsaCryptographyFactory : ICryptographyFactory
    {
        public ICryptography CreateCryptography()
            => new RsaCryptography();
    }
}