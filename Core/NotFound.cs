using System;

namespace Trustcoin.Core
{
    public class NotFound<IEntity> : ArgumentException
    {
        public NotFound(string argument) : base("NotFound", argument)
        {
        }
    }
}