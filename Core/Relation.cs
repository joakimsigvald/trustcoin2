using static Trustcoin.Core.Constants;

namespace Trustcoin.Core
{
    public class Relation
    {
        public static readonly Weight BaseWeight = 0.1f;

        public Relation(IAgent agent) : this(agent, BaseWeight)
        {
        }

        public Relation(IAgent agent, Weight weight)
        {
            Agent = agent;
            Weight = weight;
        }

        public IAgent Agent { get; }
        public Weight Weight { get; set; }
        public bool IsConnected => Agent != null;

        public bool IsEndorced
        {
            get => Agent.IsEndorced;
            set
            {
                if (IsEndorced) return;
                Agent.IsEndorced = true;
                Weight = Weight.Increase(EndorcementFactor);
            }
        }

        public override string ToString() => $"{Agent}: {Weight}";
    }
}