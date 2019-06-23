namespace Trustcoin.Core
{
    public interface IPeer : IAgent
    {
        void Endorce(IAccount account);
        bool Endorces(string name);
        float Trust { get; set; }
        float IncreaseTrust(float factor);
        float ReduceTrust(float factor);
        void UpdateRelations(IAgent sourceAgent);
    }
}