namespace Trustcoin.Core.Actions
{
    public class SignedAction : ISignedAction
    {
        public ISignature Signature { get; set; }
        public ISignature OldSignature { get; set; }
        public IAction Action { get; set; }
    }
}