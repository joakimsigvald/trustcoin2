using System.Collections.Generic;

namespace Trustcoin.Business
{
    public interface IPeer
    {
        string Name { get; }
        bool IsEndorced { get; }
        ICollection<IAgent> Relations { get; }
        void Endorce(IAccount account);
        bool Endorces(string name);
        float Trust { get; set; }
        float IncreaseTrust(float factor);
        float ReduceTrust(float factor);
    }
}