using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using static Common.Utils;
using Category = Holdem.Core.PokerHandCategory;

namespace Holdem.Core
{
    public readonly struct PokerHandRanking
        : IEquatable<PokerHandRanking>,
            IComparable<PokerHandRanking>
    {
        public Category Category { get; }
        public int[] Kickers { get; }

        private PokerHandRanking(Category category, int[] kickers)
        {
            Category = category;

            Kickers = kickers;
        }

        public static PokerHandRanking BestRanking(IEnumerable<Card> cards)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(cards.Count(), 5);

            return Combine(cards.ToList(), k: 5).Max(FromHand);
        }

        public static IEnumerable<Card> BestHand(IEnumerable<Card> cards)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(cards.Count(), 5);

            return Combine(cards.ToList(), k: 5).MaxBy(FromHand);
        }

        public static PokerHandRanking FromHand(IEnumerable<Card> hand)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(hand.Count(), 5);

            int rankMask = 0;
            var rankCount = new int[15]; // <- Indices 0, 1 unused.
            var suitCount = new int[4];

            foreach (var card in hand)
            {
                int r = (int)card.Rank;
                int s = (int)card.Suit;

                rankMask |= 1 << r;
                rankCount[r]++;
                suitCount[s]++;
            }

            var isFlush = suitCount.Any(c => c == 5);
            int straightHigh = GetStraightHigh(rankMask);

            // Straight / Royal flush.
            if (isFlush && straightHigh > 0)
            {
                return straightHigh == 14
                    ? new(Category.RoyalFlush, [14])
                    : new(Category.StraightFlush, [straightHigh]);
            }

            int four = 0;
            int three = 0;
            var pairs = new List<int>();
            var singles = new List<int>();

            for (int r = 14; r >= 2; r--)
            {
                // csharpier-ignore
                switch (rankCount[r])
                {
                    case 4: four = r; break;
                    case 3: three = r; break;
                    case 2: pairs.Add(r); break;
                    case 1: singles.Add(r); break;
                }
            }

            // Four of a kind.
            if (four > 0)
            {
                return new(Category.FourOfAKind, [four, singles[0]]);
            }

            // Full House.
            if (three > 0 && pairs.Count == 1)
            {
                return new(Category.FullHouse, [three, pairs[0]]);
            }

            // Flush.
            if (isFlush)
            {
                return new(Category.Flush, GetRanksDescending(rankMask));
            }

            // Straight.
            if (straightHigh > 0)
            {
                return new(Category.Straight, [straightHigh]);
            }

            // Three of a kind.
            if (three > 0)
            {
                return new(Category.ThreeOfAKind, [three, singles[0], singles[1]]);
            }

            // Two pair.
            if (pairs.Count == 2)
            {
                return new(Category.TwoPair, [pairs[0], pairs[1], singles[0]]);
            }

            // One pair.
            if (pairs.Count == 1)
            {
                return new(Category.OnePair, [pairs[0], singles[0], singles[1], singles[2]]);
            }

            // High card.
            return new(Category.HighCard, GetRanksDescending(rankMask));
        }

        private static int GetStraightHigh(int mask)
        {
            // Wheel: A-2-3-4-5
            int wheelMask = (1 << 14) | (1 << 5) | (1 << 4) | (1 << 3) | (1 << 2);
            if ((mask & wheelMask) == wheelMask)
            {
                return 5;
            }

            // Regular straights.
            for (int high = 14; high >= 5; high--)
            {
                int straightMask =
                    0
                    | (1 << high)
                    | (1 << (high - 1))
                    | (1 << (high - 2))
                    | (1 << (high - 3))
                    | (1 << (high - 4));

                if ((mask & straightMask) == straightMask)
                {
                    return high;
                }
            }

            return 0;
        }

        private static int[] GetRanksDescending(int mask)
        {
            var ranks = new int[5];

            for (int i = 0, r = 14; r >= 2; r--)
            {
                if ((mask & (1 << r)) != 0)
                {
                    ranks[i++] = r;
                }
            }

            return ranks;
        }

        public static bool operator ==(PokerHandRanking left, PokerHandRanking right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PokerHandRanking left, PokerHandRanking right)
        {
            return left.Equals(right) == false;
        }

        public override bool Equals(object o)
        {
            return o is PokerHandRanking other && Equals(other);
        }

        public bool Equals(PokerHandRanking other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(PokerHandRanking other)
        {
            if (Category != other.Category)
            {
                return Category.CompareTo(other.Category);
            }

            foreach (var (left, right) in Kickers.Zip(other.Kickers))
            {
                if (left != right)
                {
                    return left - right;
                }
            }

            return 0;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(Category);

            foreach (var k in Kickers)
            {
                hash.Add(k);
            }

            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"{Category} {Kickers.AsString()}";
        }
    }
}
