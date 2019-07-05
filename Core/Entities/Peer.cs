using System.Collections.Generic;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Peer : Agent, IPeer
    {
        private readonly IDictionary<string, IArtefact> _artefacts = new Dictionary<string, IArtefact>();

        internal Peer(string name, byte[] publicKey, IEnumerable<Relation> relations)
            : base(name, publicKey, relations)
        {
        }

        public SignedWeight Trust { get; set; }
        public Money Money { get; set; }

        public void Endorce()
        {
            if (IsEndorced) return;
            IsEndorced = true;
            Trust = Trust.Increase(EndorcementFactor);
        }

        public void AddArtefact(IArtefact artefact)
        {
            if (HasArtefact(artefact.Name))
                throw new DuplicateKey<string>(artefact.Name);
            _artefacts.Add(artefact.Name, artefact);
        }

        public bool HasArtefact(string name)
            => _artefacts.ContainsKey(name);

        public IArtefact GetArtefact(string name)
            => _artefacts.TryGetValue(name, out var artefact)
            ? artefact
            : throw new NotFound<IArtefact>(name);
    }
}