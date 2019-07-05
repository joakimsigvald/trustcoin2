using System;
using Trustcoin.Core.Exceptions;

namespace Trustcoin.Core.Types
{
    public struct SignedWeight : IEquatable<SignedWeight>
    {
        private readonly float _value;

        public static readonly SignedWeight Max = new SignedWeight(1f);

        public SignedWeight(float value)
        {
            _value = value < -1 || value > 1
                ? throw new OutOfBounds<float>(value)
                : value;
        }

        public static explicit operator SignedWeight(float value) => new SignedWeight(value);
        public static implicit operator float(SignedWeight sweight) => Math.Max(0, sweight._value);

        public SignedWeight Increase(Weight factor)
        {
            var norm = NormalizedValue;
            var rest = Math.Min(0.5f, 1 - norm);
            return Denormalize(norm + factor * rest / 2);
        }

        public SignedWeight Decrease(Weight factor)
        {
            var norm = NormalizedValue;
            var rest = Math.Min(0.5f, norm);
            return Denormalize(norm - factor * rest / 2);
        }

        private float NormalizedValue => (_value + 1) / 2;

        private SignedWeight Denormalize(float norm) => (SignedWeight)(2 * norm - 1);

        public override string ToString() => $"{_value}";

        public bool Equals(SignedWeight other)
            => other._value == _value;
    }
}