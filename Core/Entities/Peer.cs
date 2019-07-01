using System.Collections.Generic;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Peer : Agent, IPeer
    {
        internal Peer(string name, byte[] publicKey, IEnumerable<Relation> relations)
            : base(name, publicKey, relations)
        {
        }

        public Weight Trust { get; set; }
        public Money Money { get; set; }

        public void Endorce()
        {
            if (IsEndorced) return;
            IsEndorced = true;
            Trust = Trust.Increase(EndorcementFactor);
        }
    }
}