using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;

namespace Holdem.Core
{
    public class DeckEmptyException : Exception { }

    public class Deck
    {
        private readonly Card[] _cards = null;

        private int _index = 0;

        public Deck(bool shuffle = true)
        {
            _cards = MakeDeck(shuffle);
        }

        public Deck(IEnumerable<Card> cards)
        {
            _cards = cards.ToArray();
        }

        public bool IsEmpty => Count == 0;
        public int Count => _cards.Length - _index;

        public Deck Reset()
        {
            _index = 0;

            return this;
        }

        public void Shuffle()
        {
            _cards.Shuffle();
        }

        public Card Draw()
        {
            if (_index >= _cards.Length)
            {
                throw new DeckEmptyException();
            }

            return _cards[_index++];
        }

        public IEnumerable<Card> Draw(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return Draw();
            }
        }

        private static Card[] MakeDeck(bool shuffle)
        {
            var cards = new Card[52];
            int i = 0;

            foreach (var suit in Enum.GetValues<Suit>())
            {
                foreach (var rank in Enum.GetValues<Rank>())
                {
                    cards[i++] = new(rank, suit);
                }
            }

            if (shuffle)
            {
                cards.Shuffle();
            }

            return cards;
        }
    }
}
