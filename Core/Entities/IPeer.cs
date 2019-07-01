﻿using System.Collections.Generic;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public interface IPeer : IAgent
    {
        void Endorce();
        bool Endorces(string name);
        Weight Trust { get; set; }
        Money Money { get; set; }

        Relation AsRelation() => new Relation(Clone());
        Weight IncreaseTrust(Weight factor) => Trust = Trust.Increase(factor);
        Weight ReduceTrust(Weight factor) => Trust = Trust.Reduce(factor);
    }
}