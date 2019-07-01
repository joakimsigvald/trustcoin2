using System.Collections.Generic;
using System.Linq;
using static Trustcoin.Core.Constants;

namespace Trustcoin.Core
{
    public class Peer : Agent, IPeer
    {
        private Peer(string name, byte[] publicKey, IEnumerable<Relation> relations)
            : base(name, publicKey, relations)
        {
        }

        public static IPeer GetSelf(IAccount target)
            => new Peer(target.Name, target.PublicKey, target.Peers.Select(p => p.AsRelation()))
            {
                Trust = Weight.Max
            };

        public static IPeer MakePeer(IAgent target)
            => new Peer(target.Name, target.PublicKey, target.Relations)
            {
                Trust = BaseTrust,
                RelationWeight = BaseRelationWeight
            };

        public Weight Trust { get; set; }
        public Money Money { get; set; }
        public Weight RelationWeight { get; set; }

        public void Endorce()
        {
            if (IsEndorced) return;
            IsEndorced = true;
            Trust = Trust.Increase(EndorcementFactor);
        }
    }
}