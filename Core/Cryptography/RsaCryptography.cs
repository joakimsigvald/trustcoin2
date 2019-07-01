using System;
using System.Security.Cryptography;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Cryptography
{
    public class RsaCryptography : ICryptography
    {
        private RSA _oldRsa = RSA.Create();
        private RSA _rsa = RSA.Create();

        public RsaCryptography()
        {
            RenewKeys();
        }

        public ISignedAction Sign(IAction action) => new SignedAction
        {
            Action = action,
            Signature = new RsaSignature(action.Serialize(), _rsa),
            OldSignature = new RsaSignature(action.Serialize(), _oldRsa)
        };

        public void VerifySignature(ISignedAction action, IPeer peer)
        {
            if (!action.Signature.Verify(action.Action.Serialize(), peer.PublicKey)
                && !action.OldSignature.Verify(action.Action.Serialize(), peer.PublicKey))
                throw new InvalidOperationException("Source Signature is not valid");
        }

        public byte[] PublicKey { get; set; }

        public void RenewKeys()
        {
            _oldRsa = _rsa;
            _rsa = RSA.Create();
            PublicKey = _rsa.ExportRSAPublicKey();
        }
    }
}