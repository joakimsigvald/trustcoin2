using System;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class EndorceArtefactAction : ArtefactAction
    {
        public EndorceArtefactAction(IArtefact artefact) : base(artefact)
        {
        }
    }
}