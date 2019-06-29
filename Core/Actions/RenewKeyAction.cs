namespace Trustcoin.Core.Actions
{
    public class RenewKeyAction : BaseAction
    {
        public RenewKeyAction(ISignature signature, string oldKey, string newKey) : base(signature)
        {
            OldKey = oldKey;
            NewKey = newKey;
        }

        public string OldKey { get; }
        public string NewKey { get; }
    }
}