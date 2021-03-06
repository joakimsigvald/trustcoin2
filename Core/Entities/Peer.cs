﻿using System;
using System.Collections.Generic;
using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using static Trustcoin.Core.Entities.Constants;

namespace Trustcoin.Core.Entities
{
    public class Peer : Agent, IPeer
    {
        private readonly IDictionary<ArtefactId, Artefact> _artefacts = new Dictionary<ArtefactId, Artefact>();

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
            if (HasArtefact(artefact.Id))
                throw new DuplicateKey<ArtefactId>(artefact.Id);
            if (artefact.OwnerId != Id)
                throw new ArgumentException("Cannot add artefact with othe owner");
            _artefacts.Add(artefact.Id, artefact);
        }

        public void RemoveArtefact(ArtefactId id)
        {
            _artefacts.Remove(id);
        }

        public bool HasArtefact(ArtefactId id)
            => _artefacts.ContainsKey(id);

        public Artefact GetArtefact(ArtefactId id)
            => _artefacts.TryGetValue(id, out var artefact)
            ? artefact
            : throw new NotFound<Artefact>(nameof(id), id.ToString());

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