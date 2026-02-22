using System;
using System.Linq;

namespace Holdem.Core
{
    public readonly struct Card(Rank rank, Suit suit) : IComparable<Card>
    {
        public Rank Rank { get; } = rank;
        public Suit Suit { get; } = suit;

        public static Card Parse(string s) =>
            TryParse(s, out var card) ? card : throw new ArgumentException($"Unable to parse: {s}");

        public static bool TryParse(string s, out Card card)
        {
            if (Array.IndexOf(Deck, s) < 0)
            {
                card = default;
                return false;
            }

            Rank rank;
            Suit suit;

            if (s.StartsWith("10"))
            {
                suit = ToSuit(s.Last());
                card = new(Rank.Ten, suit);
                return true;
            }

            rank = ToRank(s.First());
            suit = ToSuit(s.Last());
            card = new(rank, suit);
            return true;
        }

        public int CompareTo(Card other)
        {
            return Rank.CompareTo(other.Rank) != 0
                ? Rank.CompareTo(other.Rank)
                : Suit.CompareTo(other.Suit);
        }

        public override string ToString()
        {
            return Deck[GetHashCode()];
        }

        public override int GetHashCode()
        {
            return (int)Suit * 13 + (int)Rank - 2;
        }

        // csharpier-ignore
        private static readonly string[] Deck =
        [
            "2s", "3s", "4s", "5s", "6s", "7s", "8s", "9s", "10s", "Js", "Qs", "Ks", "As",
            "2h", "3h", "4h", "5h", "6h", "7h", "8h", "9h", "10h", "Jh", "Qh", "Kh", "Ah",
            "2d", "3d", "4d", "5d", "6d", "7d", "8d", "9d", "10d", "Jd", "Qd", "Kd", "Ad",
            "2c", "3c", "4c", "5c", "6c", "7c", "8c", "9c", "10c", "Jc", "Qc", "Kc", "Ac"
        ];

        private static Rank ToRank(char c)
        {
            return c switch
            {
                '2' => Rank.Two,
                '3' => Rank.Three,
                '4' => Rank.Four,
                '5' => Rank.Five,
                '6' => Rank.Six,
                '7' => Rank.Seven,
                '8' => Rank.Eight,
                '9' => Rank.Nine,
                'J' => Rank.Jack,
                'Q' => Rank.Queen,
                'K' => Rank.King,
                'A' => Rank.Ace,
                _ => throw new ArgumentException($"Invalid character: '{c}'"),
            };
        }

        private static Suit ToSuit(char c)
        {
            return c switch
            {
                's' => Suit.Spades,
                'h' => Suit.Hearts,
                'd' => Suit.Diamonds,
                'c' => Suit.Clubs,
                _ => throw new ArgumentException($"Invalid character: '{c}'"),
            };
        }
    }
}
