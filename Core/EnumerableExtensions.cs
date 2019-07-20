using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Core
{
    public static class EnumerableExtensions
    {
        public static float WeightedMean(this IEnumerable<(float weight, float value)> values)
        {
            var orderedValues = values.OrderBy(v => v.value).ToArray();
            var len = orderedValues.Length;
            if (len == 0)
                throw new InvalidOperationException();
            if (len == 1)
                return orderedValues.Single().value;
            var totalWeight = orderedValues.Sum(wv => wv.weight);
            var halfTotalWeight = totalWeight / 2;
            var sum = 0f;
            var midMin = orderedValues.SkipWhile(wv => (sum += wv.weight) <= halfTotalWeight).First().value;
            sum = 0f;
            var midMax = orderedValues.SkipWhile(wv => (sum += wv.weight) < halfTotalWeight).First().value;
            return (midMin + midMax) / 2;
        }

        public static float WeightedAverage(this IEnumerable<(float weight, float value)> values)
        {
            var orderedValues = values.OrderBy(v => v.value).ToArray();
            var len = orderedValues.Length;
            if (len == 0)
                throw new InvalidOperationException();
            if (len == 1)
                return orderedValues.Single().value;
            var totalWeight = orderedValues.Sum(wv => wv.weight);
            return values.Sum(v => v.value * v.weight) / totalWeight;
        }

        public static TValue SafeGetValue<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key)
            => map.TryGetValue(key, out var value) ? value : default;
    }
}
