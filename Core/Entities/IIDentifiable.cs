namespace Trustcoin.Core.Entities
{
    public interface IIDentifiable<TId>
    {
        TId Id { get; }
    }
}