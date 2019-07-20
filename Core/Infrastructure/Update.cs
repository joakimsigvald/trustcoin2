using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Entities;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Infrastructure
{
    public class Update
    {
        public Update(IEnumerable<IHolder> holders, IEnumerable<Artefact> artefacts)
        {
            PeerMoney = holders.ToDictionary(p => p.Id, p => p.Money);
            Artefacts = artefacts.ToArray();
        }

        public IDictionary<AgentId, Money> PeerMoney { get; }
        public Artefact[] Artefacts { get; }
    }
}