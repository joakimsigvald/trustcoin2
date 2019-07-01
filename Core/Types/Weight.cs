using Trustcoin.Core.Exceptions;

namespace Trustcoin.Core.Types
{
    public struct Weight
    {
        private readonly float _value;

        public static readonly Weight Max = (Weight)1f;

        public Weight(float value)
        {
            _value = value < 0 || value > 1
                ? throw new OutOfBounds<float>(value)
                : value;
        }

        public static explicit operator Weight(float value) => new Weight(value);
        public static implicit operator float(Weight weight) => weight._value;
        public static Weight operator +(Weight w1, Weight w2) => (Weight)(w1._value + w2._value);
        public static float operator +(int n, Weight w2) => (Weight)(n + w2._value);
        public static Weight operator -(Weight w1, Weight w2) => (Weight)(w1._value - w2._value);
        public static Weight operator -(int n, Weight w2) => (Weight)(n - w2._value);
        public static Weight operator *(Weight w1, Weight w2) => (Weight)(w1._value * w2._value);
        public static Weight operator *(Weight w1, float f) => (Weight)(w1._value * f);

        public Weight Increase(Weight factor) => (Weight)(_value + (1 - _value) * factor);
        public Weight Reduce(Weight factor) => (Weight)(_value * (1 - factor));

        public override string ToString() => $"{_value}";
    }
}
