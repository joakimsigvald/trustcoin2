using System.Collections.Generic;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IPeer : IAgent, IHolder
    {
        void Endorce();
        SignedWeight Trust { get; set; }
        IEnumerable<Artefact> OwnedArtefacts { get; }
        Relation AsRelation() => new Relation(Clone());
        SignedWeight IncreaseTrust(Weight factor) => Trust = Trust.Increase(factor);
        SignedWeight DecreaseTrust(Weight factor) => Trust = Trust.Decrease(factor);
        void AddArtefact(Artefact artefact);
        bool HasArtefact(ArtefactId id);
        Artefact GetArtefact(ArtefactId id);
        void RemoveArtefact(Artefact artefact);
        void IncreaseMoney(Money money);
        void DecreaseMoney(Money money);
    }
}