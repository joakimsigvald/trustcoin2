using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public abstract class ArtefactAction : ActionBase
    {
        public ArtefactAction(IArtefact artefact)
        {
            Artefact = artefact;
        }

        protected ArtefactAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var name = (string)info.GetValue(nameof(Artefact.Name), typeof(string));
            var ownerName = (string)info.GetValue(nameof(Artefact.OwnerName), typeof(string));
            Artefact = new Artefact(name, ownerName);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Artefact.Name), Artefact.Name);
            info.AddValue(nameof(Artefact.OwnerName), Artefact.OwnerName);
        }

        public IArtefact Artefact { get; }
    }
}