﻿using System;
using System.Collections.Generic;

namespace Trustcoin.Core
{

    public class Network : INetwork
    {
        private readonly IDictionary<string, Account> _accounts = new Dictionary<string, Account>();

        public IAccount CreateAccount(string name)
            => _accounts[name] = new Account(this, name);

        public IAgent FindAgent(string name)
            => _accounts.TryGetValue(name, out var account) ? new Agent(account) : null;

        public bool Update(string targetName, string sourceName, ISignature sourceSignature)
        {
            var targetAccount = GetAccount(targetName);
            var sourceAgent = FindAgent(sourceName);
            if (!sourceAgent.IsConnectedTo(targetName))
                throw new InvalidOperationException("Source is not connected to target");
            if (!sourceSignature.Verify(sourceName))
                throw new InvalidOperationException("Source Signature is not valid");
            return targetAccount.IsConnectedTo(sourceName)
                && targetAccount.Update(sourceAgent, sourceSignature);
        }

        private IAccount GetAccount(string name)
            => _accounts[name];
    }
}