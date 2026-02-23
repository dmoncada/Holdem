using System;
using System.Linq;
using Xunit;

namespace Holdem.Core.Tests
{
    public class PokerHandRankingTests
    {
        [Theory]
        [InlineData("As Ks Qs Js")]
        [InlineData("As Ks Qs Js 10s 9s")]
        public void TestHand_NotExactlyFiveCards(string s)
        {
            var hand = s.Split(' ').Select(Card.Parse);

            Assert.Throws<ArgumentOutOfRangeException>(() => PokerHandRanking.FromHand(hand));
        }

        [Theory]
        [InlineData("As Ks Qs Js 10s", PokerHandCategory.RoyalFlush)]
        [InlineData("Ks Qs Js 10s 9s", PokerHandCategory.StraightFlush)]
        [InlineData("Ks Ks Ks Ks 9s", PokerHandCategory.FourOfAKind)]
        [InlineData("Ks Ks Ks 9s 9h", PokerHandCategory.FullHouse)]
        [InlineData("As Ks Qs Js 9s", PokerHandCategory.Flush)]
        [InlineData("As Kh Qd Jc 10s", PokerHandCategory.Straight)]
        public void TestHand_ExactlyFiveCards(string s, PokerHandCategory category)
        {
            var hand = s.Split(' ').Select(Card.Parse);
            var rank = PokerHandRanking.FromHand(hand);

            Assert.Equal(category, rank.Category);
        }

        [Theory]
        [InlineData("As Ks Qs Js 10s 9s 9h", PokerHandCategory.RoyalFlush)]
        public void TestRanking_BestHand(string s, PokerHandCategory category)
        {
            var cards = s.Split(' ').Select(Card.Parse);
            var rank = PokerHandRanking.BestRanking(cards);

            Assert.Equal(category, rank.Category);
        }

        [Theory]
        [InlineData("4h 4c 4d 6s 7s 8s", "2s 2h 3s 3h 4s 5s")]
        public void TestRanking_Compare(string s1, string s2)
        {
            var hand1 = s1.Split(' ').Select(Card.Parse);
            var hand2 = s2.Split(' ').Select(Card.Parse);
            var rank1 = PokerHandRanking.BestRanking(hand1);
            var rank2 = PokerHandRanking.BestRanking(hand2);

            Assert.True(rank1.CompareTo(rank2) > 0);
            Assert.NotEqual(rank1, rank2);
        }

        [Theory]
        [InlineData("Qd Ks 8c Ah 9c", "Qs Kh 8c Ah 9c")]
        public void TestRanking_CompareEqual(string s1, string s2)
        {
            var hand1 = s1.Split(' ').Select(Card.Parse);
            var hand2 = s2.Split(' ').Select(Card.Parse);
            var rank1 = PokerHandRanking.BestRanking(hand1);
            var rank2 = PokerHandRanking.BestRanking(hand2);

            Assert.Equal(0, rank1.CompareTo(rank2));
            Assert.Equal(rank1, rank2);
        }

        [Theory]
        [InlineData("2s 2h 2d 9s 10s", "2s 2h 2d 9h 10h", "2s 2h 2d 3s 4s")]
        public void TestRanking_Sort(string s1, string s2, string s3)
        {
            var cards = new string[] { s1, s2, s3 };
            var hands = cards.Select(c => c.Split(' ').Select(Card.Parse));
            var ranks = hands.Select(PokerHandRanking.FromHand).OrderDescending();
            var max = ranks.Max();
            var min = ranks.Min();

            Assert.Equal(ranks.First(), max);
            Assert.Equal(ranks.Last(), min);

            // 2x -> Trips [2, 9, 10]
            // 1x -> Trips [2, 3, 4]

            Assert.Equal(2, ranks.Count(r => r == max));
            Assert.Equal(1, ranks.Count(r => r == min));
        }
    }
}
