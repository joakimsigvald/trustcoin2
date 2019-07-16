using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class CreateArtefactAction : ArtefactAction
    {
        public CreateArtefactAction(Artefact artefact) : base(artefact)
        {
        }

        protected CreateArtefactAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}