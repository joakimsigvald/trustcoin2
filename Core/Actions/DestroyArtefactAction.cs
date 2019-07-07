using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class DestroyArtefactAction : ArtefactAction
    {
        public DestroyArtefactAction(IArtefact artefact) : base(artefact)
        {
        }

        protected DestroyArtefactAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}