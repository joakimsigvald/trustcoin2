using Trustcoin.Core.Exceptions;

namespace Trustcoin.Core.Types
{
    public struct Money
    {
        public static readonly Money Min = (Money)0f;

        public readonly float Value;

        public Money(float value)
        {
            Value = value < 0
                ? throw new OutOfBounds<float>(value)
                : value;
        }

        public static explicit operator Money(float value) => new Money(value);
        public static implicit operator float(Money money) => money.Value;
        public static Money operator +(Money m1, Money m2) => (Money)(m1.Value + m2.Value);
        public static Money operator *(float n, Money m) => (Money)(n * m.Value);
        public static Money operator -(Money m1, Money m2) => (Money)(m1.Value - m2.Value);

        public override string ToString() => $"${Value}";
    }
}
