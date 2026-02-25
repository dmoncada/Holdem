using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Holdem.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static void Print<T>(
            this IEnumerable<T> enumerable,
            string format = "[{0}]",
            TextWriter outputWriter = null
        )
        {
            outputWriter ??= Console.Out;

            outputWriter.WriteLine(enumerable.AsString(format));
        }

        public static string AsString<T>(this IEnumerable<T> enumerable, string format = "[{0}]")
        {
            return string.Format(format, string.Join(", ", enumerable));
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T toRemoveItem)
        {
            return enumerable.Where(x => x.Equals(toRemoveItem) == false);
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, int toRemoveIndex)
        {
            int count = enumerable.Count();
            var left = enumerable.Take(toRemoveIndex);
            var right = enumerable.TakeLast(count - toRemoveIndex - 1);
            return left.Concat(right);
        }

        public static IEnumerable<T> Rotate<T>(this IEnumerable<T> enumerable, int k)
        {
            return enumerable.Skip(k).Concat(enumerable.Take(k));
        }

        public static IEnumerable<(T1, T2)> Zip<T1, T2>(
            IEnumerable<T1> enumerable1,
            IEnumerable<T2> enumerable2
        )
        {
            using var e1 = enumerable1.GetEnumerator();
            using var e2 = enumerable2.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext())
            {
                yield return (e1.Current, e2.Current);
            }
        }

        public static IEnumerable<(T1, T2, T3)> Zip<T1, T2, T3>(
            IEnumerable<T1> enumerable1,
            IEnumerable<T2> enumerable2,
            IEnumerable<T3> enumerable3
        )
        {
            using var e1 = enumerable1.GetEnumerator();
            using var e2 = enumerable2.GetEnumerator();
            using var e3 = enumerable3.GetEnumerator();

            while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
            {
                yield return (e1.Current, e2.Current, e3.Current);
            }
        }

        public static TSource MaxBy<TSource, TKey>(
            this IEnumerable<TSource> enumerable,
            Func<TSource, TKey> selector
        )
            where TKey : IComparable<TKey>
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            using var enumerator = enumerable.GetEnumerator();

            if (enumerator.MoveNext() == false)
                throw new InvalidOperationException("Collection empty.");

            var max = enumerator.Current;
            var maxKey = selector(max);

            while (enumerator.MoveNext())
            {
                var candidate = enumerator.Current;
                var candidateKey = selector(candidate);

                if (candidateKey.CompareTo(maxKey) > 0)
                {
                    max = candidate;
                    maxKey = candidateKey;
                }
            }

            return max;
        }

        public static List<T> ManyMaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
            where TKey : IComparable<TKey>
        {
            using var e = source.GetEnumerator();

            if (e.MoveNext() == false)
                throw new InvalidOperationException("Collection empty.");

            var maxItems = new List<T> { e.Current };
            var maxKey = selector(e.Current);

            while (e.MoveNext())
            {
                var candidate = e.Current;
                var candidateKey = selector(candidate);
                int comparison = candidateKey.CompareTo(maxKey);

                if (comparison > 0)
                {
                    maxItems.Clear();
                    maxItems.Add(candidate);
                    maxKey = candidateKey;
                }
                if (comparison == 0)
                {
                    maxItems.Add(candidate);
                }
            }

            return maxItems;
        }
    }
}
