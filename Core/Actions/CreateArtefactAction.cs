﻿using System;
using Trustcoin.Core.Entities;

namespace Trustcoin.Core.Actions
{
    [Serializable]
    public class CreateArtefactAction : ArtefactAction
    {
        public CreateArtefactAction(IArtefact artefact) : base(artefact)
        {
        }
    }
}