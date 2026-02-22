using System;
using System.Collections.Generic;

namespace Common.Extensions
{
    public static class ListExtensions
    {
        private static readonly Random Random = new(Guid.NewGuid().GetHashCode());

        public static List<T> Sorted<T>(this List<T> list)
        {
            list.Sort();
            return list;
        }

        public static List<T> Sorted<T>(this List<T> list, Comparison<T> comparison)
        {
            list.Sort(comparison);
            return list;
        }

        public static List<T> Sorted<T>(this List<T> list, IComparer<T> comparer)
        {
            list.Sort(comparer);
            return list;
        }

        public static List<T> Reversed<T>(this List<T> list)
        {
            list.Reverse();
            return list;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                int k = Random.Next(n--);

                // Swap n <-> k.
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        public static IList<T> Rotated<T>(this IList<T> array, int k, bool left = true)
        {
            int n = array.Count;
            k %= n;

            if (k > 0)
            {
                if (left)
                    k = n - k;

                return RotatedRight(array, k);
            }

            return array;
        }

        private static IList<T> RotatedRight<T>(IList<T> array, int k)
        {
            int n = array.Count;
            ReversePartial(array, 0, n - 1);
            ReversePartial(array, 0, k - 1);
            ReversePartial(array, k, n - 1);
            return array;
        }

        private static void ReversePartial<T>(IList<T> numbers, int start, int end)
        {
            while (start < end)
            {
                // Swap start <-> end.
                (numbers[start], numbers[end]) = (numbers[end], numbers[start]);

                start++;
                end--;
            }
        }
    }
}
