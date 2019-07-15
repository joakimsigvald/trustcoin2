using System;
using System.Collections.Generic;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Peer : Agent, IPeer
    {
        private readonly IDictionary<string, IArtefact> _artefacts = new Dictionary<string, IArtefact>();

        internal Peer(string name, AgentId id, byte[] publicKey, IEnumerable<Relation> relations)
            : base(name, id, publicKey, relations)
        {
        }

        public SignedWeight Trust { get; set; }
        public Money Money { get; set; }
        public IEnumerable<IArtefact> OwnedArtefacts => _artefacts.Values;

        public void Endorce()
        {
            Trust = Trust.Increase(EndorcementTrustFactor);
        }

        public void AddArtefact(IArtefact artefact)
        {
            if (HasArtefact(artefact.Name))
                throw new DuplicateKey<string>(artefact.Name);
            if (artefact.OwnerName != Name)
                throw new ArgumentException("Cannot add artefact with othe owner");
            _artefacts.Add(artefact.Name, artefact);
        }

        public void RemoveArtefact(IArtefact artefact)
        {
            _artefacts.Remove(artefact.Name);
        }

        public bool HasArtefact(string name)
            => _artefacts.ContainsKey(name);

        public IArtefact GetArtefact(string name)
            => _artefacts.TryGetValue(name, out var artefact)
            ? artefact
            : throw new NotFound<IArtefact>(name);
    }
}