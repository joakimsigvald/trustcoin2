namespace Trustcoin.Core.Actions
{
    public interface IAction
    {
        ISignature SourceSignature { get; }
    }
}