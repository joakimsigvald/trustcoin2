using System;

namespace Trustcoin.Business
{
    public class NotFound<IEntity> : ArgumentException
    {
        public NotFound(string argument) : base("NotFound", argument)
        {
        }
    }
}