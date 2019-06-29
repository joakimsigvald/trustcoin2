namespace Trustcoin.Core.Cryptography
{
    public class SimpleCryptographyFactory : ICryptographyFactory
    {
        public ICryptography CreateCryptography()
            => new SimpleCryptography();
    }
}