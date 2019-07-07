using System;
using Trustcoin.Core.Cryptography;

namespace Trustcoin.Core.Actions
{
    public class SignedAction
    {
        public ISignature Signature { get; set; }
        public ISignature OldSignature { get; set; }
        public IAction Action { get; set; }

        public SignedAction Clone()
            => new SignedAction
            {
                Signature = Signature,
                OldSignature = OldSignature,
                Action = Action.Clone()
            };
    }
}