using System;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class DestroyArtefactAction : ArtefactAction
    {
        public DestroyArtefactAction(IArtefact artefact) : base(artefact)
        {
        }
    }
}