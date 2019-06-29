namespace Trustcoin.Core
{
    public interface IPeer : IAgent
    {
        void Endorce();
        bool Endorces(string name);
        Weight Trust { get; set; }
        Relation AsRelation() => new Relation(Clone());
        Weight IncreaseTrust(Weight factor) => Trust = Trust.Increase(factor);
        Weight ReduceTrust(Weight factor) => Trust = Trust.Reduce(factor);
    }
}