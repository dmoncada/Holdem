using System;
using System.Collections.Generic;
using System.Linq;
using Holdem.Common.Extensions;

namespace Holdem.Core
{
    public class DeckEmptyException : Exception { }

    public class Deck
    {
        private readonly Card[] _cards = null;

        private int _index = 0;

        public Deck()
        {
            _cards = MakeDeck();
        }

        public Deck(IEnumerable<Card> cards)
        {
            _cards = cards.ToArray();
        }

        public bool IsEmpty => Count == 0;
        public int Count => _cards.Length - _index;

        public void Reset()
        {
            _index = 0;
        }

        public virtual void Shuffle()
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

        private static Card[] MakeDeck()
        {
            var cards = new Card[52];
            int i = 0;

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    cards[i++] = new Card(rank, suit);
                }
            }

            return cards;
        }
    }
}
