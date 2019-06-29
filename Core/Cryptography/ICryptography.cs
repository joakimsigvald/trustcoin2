﻿using Trustcoin.Core.Actions;

namespace Trustcoin.Core.Cryptography
{
    public interface ICryptography
    {
        ISignedAction Sign(IAction action);
        void VerifySignature(ISignedAction action, IPeer peer);
        byte[] PublicKey { get; }
        void RenewKeys();
    }
}