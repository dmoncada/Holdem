using System;
using Xunit;

namespace Holdem.Core.Tests
{
    public class CardTests
    {
        [Theory]
        [InlineData("As", Rank.Ace, Suit.Spades)]
        [InlineData("Kh", Rank.King, Suit.Hearts)]
        [InlineData("Qd", Rank.Queen, Suit.Diamonds)]
        [InlineData("Jc", Rank.Jack, Suit.Clubs)]
        [InlineData("10s", Rank.Ten, Suit.Spades)]
        public void TestParsing_ValidString(string s, Rank rank, Suit suit)
        {
            var result = Card.TryParse(s, out var card);

            Assert.True(result);
            Assert.Equal(new Card(rank, suit), card);
        }

        [Theory]
        [InlineData("Ax")]
        [InlineData("1s")]
        public void TestParsing_InvalidString(string s)
        {
            Assert.False(Card.TryParse(s, out var _));
            Assert.Throws<ArgumentException>(() => Card.Parse(s));
        }
    }
}
