using Trustcoin.Core.Actions;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Cryptography
{
    public interface ICryptography
    {
        SignedAction Sign(IAction action);
        void VerifySignature(SignedAction action, IPeer peer);
        byte[] PublicKey { get; }
        void RenewKeys();
    }
}