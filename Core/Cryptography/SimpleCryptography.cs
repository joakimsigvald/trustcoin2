using System;
using Trustcoin.Core.Actions;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Cryptography
{
    public class SimpleCryptography : ICryptography
    {
        private static readonly Random _rand = new Random((int)DateTime.Now.Ticks);

        public SimpleCryptography()
        {
            RenewKeys();
        }

        public SignedAction Sign(IAction action) => new SignedAction
        {
            Action = action,
            Signature = new SimpleSignature(),
            OldSignature = new SimpleSignature()
        };

        public void VerifySignature(SignedAction action, IPeer peer)
        {
            if (!action.Signature.Verify(action.Action.Serialize(), peer.PublicKey)
                && !action.OldSignature.Verify(action.Action.Serialize(), peer.PublicKey))
                throw new InvalidOperationException("Source Signature is not valid");
        }

        public byte[] PublicKey { get; set; }

        public void RenewKeys()
        {
            PublicKey = CreateKey();
        }

        private static byte[] CreateKey() => new[] { (byte)_rand.NextDouble() };
    }
}