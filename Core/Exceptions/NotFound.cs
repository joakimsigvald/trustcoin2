using System;

namespace Trustcoin.Core.Exceptions
{
    public class NotFound<IEntity> : ArgumentException
    {
        public NotFound(string argument, string value) : base($"NotFound: {value}", argument)
        {
        }
    }
}