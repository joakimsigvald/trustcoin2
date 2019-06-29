﻿using Trustcoin.Core.Actions;

namespace Trustcoin.Core
{
    public interface IClient
    {
        bool Update(string sourceAgentName, ISignedAction action);
        bool IsConnectedTo(string name);
    }
}