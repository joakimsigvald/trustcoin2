using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
{
    public class Update
    {
        public Update(IEnumerable<IPeer> peers, IEnumerable<IArtefact> artefacts)
        {
            PeerMoney = peers.ToDictionary(p => p.Name, p => p.Money);
            ArtefactOwners = artefacts
                .Concat(peers.SelectMany(pu => pu.OwnedArtefacts))
                .Distinct()
                .ToDictionary(p => p.Name, p => p.OwnerName);
        }

        public IDictionary<string, Money> PeerMoney { get; }
        public IDictionary<string, string> ArtefactOwners { get; }
    }
}