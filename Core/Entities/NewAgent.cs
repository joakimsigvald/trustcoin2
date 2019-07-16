using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Core.Types;

namespace Trustcoin.Core.Entities
{
    public class NewAgent : INewAgent
    {
        public NewAgent(IAccount account)
            : this(account.Name, account.Id, account.PublicKey)
        {
        }

        [JsonConstructor]
        public NewAgent(string name, AgentId id, byte[] publicKey)
        {
            Name = name;
            Id = id;
            PublicKey = publicKey;
        }

        public virtual IAgent Clone()
            => new Agent(Name, Id, PublicKey);

        public string Name { get; private set; }
        public AgentId Id { get; }
        public byte[] PublicKey { get; set; }

        public override string ToString() => Name;
    }
}