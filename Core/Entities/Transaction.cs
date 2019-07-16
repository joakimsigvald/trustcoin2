﻿using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public class Transaction
    {
        public string Key { get; set; }
        public Transfer[] Transfers { get; set; }
    }

    public class Transfer
    {
        public Artefact[] Artefacts { get; set; }
        public Money Money{ get; set; }
        public string GiverName { get; set; }
        public string ReceiverName { get; set; }
    }
}