namespace Trustcoin.Core
{
    public struct Money
    {
        private readonly float _value;

        public Money(float value)
        {
            _value = value < 0
                ? throw new OutOfBounds<float>(value) 
                : value;
        }

        public static explicit operator Money(float value) => new Money(value);
        public static implicit operator float(Money money) => money._value;
        public static Money operator +(Money m1, Money m2) => (Money)(m1._value + m2._value);
        public static Money operator -(Money m1, Money m2) => (Money)(m1._value - m2._value);

        public override string ToString() => $"{_value}";
    }
}
