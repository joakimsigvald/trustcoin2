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

    public class DuplicateKey<TKey> : Exception
    {
        public DuplicateKey(TKey key)
            : base("Duplicate key: " + key)
        {
        }
    }
}