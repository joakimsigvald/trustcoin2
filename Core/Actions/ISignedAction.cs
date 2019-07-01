using Trustcoin.Core.Cryptography;

namespace Trustcoin.Core.Actions
{
    public interface ISignedAction
    {
        ISignature OldSignature { get; set; }
        ISignature Signature { get; set; }
        IAction Action { get; set; }
    }
}