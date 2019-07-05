using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class CreateArtefactAction : ActionBase
    {
        public CreateArtefactAction(IArtefact artefact)
        {
            Artefact = artefact;
        }

        protected CreateArtefactAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Artefact = new Artefact((string)info.GetValue(nameof(Artefact.Name), typeof(string)));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Artefact.Name), Artefact.Name);
        }

        public IArtefact Artefact { get; }
    }
}