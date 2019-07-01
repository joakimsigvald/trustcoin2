using System;

namespace Trustcoin.Core.Exceptions
{
    public class NotFound<IEntity> : ArgumentException
    {
        public NotFound(string argument) : base("NotFound", argument)
        {
        }
    }
}