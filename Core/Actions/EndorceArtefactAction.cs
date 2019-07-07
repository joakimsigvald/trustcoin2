using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class EndorceArtefactAction : ArtefactAction
    {
        public EndorceArtefactAction(IArtefact artefact) : base(artefact)
        {
        }

        protected EndorceArtefactAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}