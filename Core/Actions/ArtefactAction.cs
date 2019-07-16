using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public abstract class ArtefactAction : ActionBase<Artefact, Artefact>
    {
        public ArtefactAction(Artefact artefact) : base(artefact)
        {
        }

        protected ArtefactAction(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}