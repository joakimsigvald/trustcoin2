using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Relation
    {
        public Relation(IAgent agent)
        {
            Agent = agent;
        }

        public IAgent Agent { get; }
        public Weight Strength { get; set; }

        public void Endorce()
        {
            IncreaseStrength(EndorcementTrustFactor);
        }

        public void IncreaseStrength(Weight factor)
        {
            Strength = Strength.Increase(factor);
        }

        public override string ToString() => $"{Agent}: {Strength}";
    }
}