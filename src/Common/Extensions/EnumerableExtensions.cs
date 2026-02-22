using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Extensions
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

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<int, T> action)
        {
            foreach (var (index, item) in enumerable.Index())
            {
                action(index, item);
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
    }
}
