using System;
using System.Collections.Generic;
using Common.Extensions;
using Holdem.Core;

namespace Holdem.Engine
{
    public class Player(string name, int stack)
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string Name { get; } = name;
        public int Stack { get; set; } = stack;
        public bool Active { get; set; } = true;

        private readonly List<Card> _hole = [];
        public IReadOnlyList<Card> Hole => _hole;

        public bool CanAct => Active && Stack > 0;

        public void Reset()
        {
            Active = true;

            _hole.Clear();
        }

        public void TakeCard(Card card)
        {
            if (_hole.Count > 1)
            {
                throw new InvalidOperationException($"Cannot deal, {_hole.Count} cards");
            }

            _hole.Add(card);
        }

        public int Contribute(int amount)
        {
            if (Stack < amount)
            {
                throw new ArgumentException($"Cannot contribute, {Stack} < {amount}");
            }

            Stack -= amount;

            return amount;
        }

        public override string ToString()
        {
            return $"{Name} (stack: ${Stack}, folded? {Active == false}, cards: {_hole.AsString()})";
        }
    }
}
