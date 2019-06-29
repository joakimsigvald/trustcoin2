namespace Trustcoin.Core.Actions
{
    public abstract class BaseAction : IAction
    {
        protected BaseAction(ISignature signature)
        {
            SourceSignature = signature;
        }

        public ISignature SourceSignature { get; }
    }
}