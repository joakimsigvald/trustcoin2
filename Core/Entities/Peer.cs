using System;
using System.Collections.Generic;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Peer : Agent, IPeer
    {
        private readonly IDictionary<string, Artefact> _artefacts = new Dictionary<string, Artefact>();

        internal Peer(string name, AgentId id, byte[] publicKey, IEnumerable<Relation> relations)
            : base(name, id, publicKey, relations)
        {
        }

        public SignedWeight Trust { get; set; }
        public Money Money { get; set; }
        public IEnumerable<Artefact> OwnedArtefacts => _artefacts.Values;

        public void Endorce()
        {
            Trust = Trust.Increase(EndorcementTrustFactor);
        }

        public void AddArtefact(Artefact artefact)
        {
            if (HasArtefact(artefact.Name))
                throw new DuplicateKey<string>(artefact.Name);
            if (artefact.OwnerName != Name)
                throw new ArgumentException("Cannot add artefact with othe owner");
            _artefacts.Add(artefact.Name, artefact);
        }

        public void RemoveArtefact(Artefact artefact)
        {
            _artefacts.Remove(artefact.Name);
        }

        public bool HasArtefact(string name)
            => _artefacts.ContainsKey(name);

        public Artefact GetArtefact(string name)
            => _artefacts.TryGetValue(name, out var artefact)
            ? artefact
            : throw new NotFound<Artefact>(name);

        public void IncreaseMoney(Money money)
        {
            Money += money;
        }

        public void DecreaseMoney(Money money)
        {
            Money -= money;
        }
    }
}