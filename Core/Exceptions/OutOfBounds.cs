using System;

namespace Trustcoin.Core.Exceptions
{
    public class OutOfBounds<TValue> : Exception
    {
        public OutOfBounds(TValue value)
            : base("Value is out of bounds: " + value)
        {
        }
    }
}