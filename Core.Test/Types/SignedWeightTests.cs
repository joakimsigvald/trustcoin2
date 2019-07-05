using Trustcoin.Core.Exceptions;
using Trustcoin.Core.Types;
using Xunit;

namespace Trustcoin.Core.Test.Types
{
    public class SignedWeightTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(-1, 0)]
        [InlineData(0.5, 0.5)]
        public void ImplicitCastToFloat(float sweight, float expected)
        {
            var sw = new SignedWeight(sweight);
            Assert.Equal(expected, sw);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(-1, -1)]
        [InlineData(0.5, 0.5)]
        public void ExplicitCastFromFloat(float value, float expected)
        {
            var sw = new SignedWeight(expected);
            Assert.Equal(sw, (SignedWeight)value);
        }

        [Theory]
        [InlineData(1.0001)]
        [InlineData(-1.0001)]
        public void WhenCastFromFloatOutOfBounds_ThrowsOutOfBounds(float value)
        {
            Assert.Throws<OutOfBounds<float>>(() => (SignedWeight)value);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(0, 1, 0.5)]
        [InlineData(0.5, 0.4, 0.6)]
        [InlineData(0.5, 0.8, 0.7)]
        [InlineData(0.5, 1, 0.75)]
        [InlineData(-1, 1, 0)]
        [InlineData(-1, 0.5, -0.25)]
        [InlineData(-0.5, 0.4, -0.3)]
        [InlineData(-0.5, 0.8, -0.1)]
        [InlineData(-0.5, 1, 0)]
        public void CanIncreaseByFactor(float initial, float factor, float expected)
        {
            var sw = (SignedWeight)initial;
            var res = sw.Increase((Weight)factor);
            Assert.Equal((SignedWeight)expected, res, 5);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 0.5)]
        [InlineData(0, 1, -0.5)]
        [InlineData(0.5, 0.4, 0.3)]
        [InlineData(0.5, 0.8, 0.1)]
        [InlineData(0.5, 1, 0)]
        [InlineData(-1, 1, -1)]
        [InlineData(0, 0.5, -0.25)]
        [InlineData(-0.5, 0.4, -0.6)]
        [InlineData(-0.5, 0.8, -0.7)]
        [InlineData(-0.5, 1, -0.75)]
        public void CanDecreaseByFactor(float initial, float factor, float expected)
        {
            var sw = (SignedWeight)initial;
            var res = sw.Decrease((Weight)factor);
            Assert.Equal((SignedWeight)expected, res, 5);
        }
    }
}
