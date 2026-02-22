using System;
using System.Linq;
using Xunit;

namespace Holdem.Core.Tests
{
    public class DeckTests
    {
        [Fact]
        public void TestDrawSingle_AdvancesIndex()
        {
            var deck = new Deck();
            var first = deck.Draw();
            var second = deck.Draw();
            Assert.NotEqual(first, second);
        }

        [Fact]
        public void TestDrawAll_DeckEmpty()
        {
            var deck = new Deck();

            for (int i = 0, count = deck.Count; i < count; i++)
            {
                deck.Draw();
            }

            Assert.True(deck.IsEmpty);
            Assert.Throws<DeckEmptyException>(() => deck.Draw());
        }

        [Fact]
        public void TestDraw52_AllUnique()
        {
            var deck = new Deck();
            var cards = deck.Draw(52).ToList();
            Assert.Equal(52, cards.Count);
            Assert.Equal(52, cards.Distinct().Count());
        }

        [Fact]
        public void TestReset_AllowsRedrawing()
        {
            var deck = new Deck();
            int count = deck.Count;
            var one = deck.Draw();

            Assert.Equal(52, count);

            deck.Reset();
            count = deck.Count;
            var two = deck.Draw();

            Assert.Equal(52, count);
            Assert.Equal(one, two);
        }

        [Fact]
        public void TestNew_ContainsAll52Cards()
        {
            var deck = new Deck();
            var cards = deck.Draw(52).ToList();

            var expected =
                from Suit suit in Enum.GetValues<Suit>()
                from Rank rank in Enum.GetValues<Rank>()
                select new Card(rank, suit);

            Assert.True(expected.All(cards.Contains));
        }

        [Fact]
        public void TestCustom_CardsDrawnInOrder()
        {
            Card[] cards =
            [
                new Card(Rank.Ace, Suit.Spades),
                new Card(Rank.King, Suit.Spades),
                new Card(Rank.Queen, Suit.Spades),
            ];

            var deck = new Deck(cards);
            Assert.Equal(cards[0], deck.Draw());
            Assert.Equal(cards[1], deck.Draw());
            Assert.Equal(cards[2], deck.Draw());
        }

        [Fact]
        public void TestShuffle()
        {
            var deck = new Deck();
            var cards1 = deck.Draw(52).ToList();

            deck.Reset().Shuffle();
            var cards2 = deck.Draw(52).ToList();

            Assert.False(cards1.SequenceEqual(cards2));
        }
    }
}
