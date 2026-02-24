using System;
using System.Collections.Generic;

namespace Holdem.Common
{
    public static class Utils
    {
        public static bool IsEven(this int number)
        {
            return number % 2 == 0;
        }

        public static bool IsOdd(this int number)
        {
            return (number & 1) == 1;
        }

        public static bool IsMultipleOf(this int number, int divisor)
        {
            return number % divisor == 0;
        }

        public static bool IsPowerOf2(this int number)
        {
            return number.CountBits() == 1;
        }

        public static int CountBits(this int number)
        {
            int count = 0;
            while (number > 0)
            {
                number &= number - 1;
                count++;
            }
            return count;
        }

        public static List<List<T>> Combine<T>(IList<T> items, int k)
        {
            var result = new List<List<T>>();
            var current = new List<T>();

            void Backtrack(int start)
            {
                if (current.Count == k)
                {
                    result.Add(new List<T>(current));
                    return;
                }

                for (int i = start; i < items.Count; i++)
                {
                    current.Add(items[i]);
                    Backtrack(i + 1);
                    current.RemoveAt(current.Count - 1);
                }
            }

            Backtrack(0);
            return result;
        }

        public static long NumCombinations(int n, int k)
        {
            if (k < 0 || k > n)
            {
                throw new ArgumentOutOfRangeException(nameof(k));
            }

            if (k == 0 || k == n)
            {
                return 1;
            }

            k = Math.Min(k, n - k);

            long result = 1;
            // n! / k! * (n - k)!
            for (int i = 1; i <= k; i++)
            {
                checked // Throw if overflow.
                {
                    result = result * (n - k + i) / i;
                }
            }
            return result;
        }

        public static long Factorial(this int number)
        {
            long factorial = number;
            while (--number > 0)
            {
                checked
                {
                    factorial *= number;
                }
            }
            return factorial;
        }

        public static long Gcd(long a, long b, long c)
        {
            return Gcd(Gcd(a, b), c);
        }

        public static long Gcd(long a, long b)
        {
            while (b != 0)
            {
                (a, b) = (b, a % b);
            }

            return a;
        }
    }
}
