using System;
using System.Collections.Generic;
using System.Linq;

namespace Holdem.Engine
{
    public class PokerTable(IEnumerable<Player> players, int button = 0) // : IEnumerator<Player>
    {
        private readonly List<Player> _players = [.. players];

        private int _button = button;
        private int _index = button;

        public IEnumerable<Player> All => _players;
        public IEnumerable<Player> AllActive => _players.Where(p => p.Active);
        public IEnumerable<Player> AllActiveWithStack => _players.Where(p => p.CanAct);
        public bool IsHeadsUp => _players.Count(p => p.CanAct) == 2;
        public Player Current => _players[_index];

        public void Reset()
        {
            _index = _button;
        }

        public bool MoveNext()
        {
            int next = NextActive(_index);
            if (next < 0)
            {
                return false;
            }

            _index = next;
            return true;
        }

        public void MoveButton()
        {
            int next = NextActive(_button);
            if (next < 0)
            {
                throw new InvalidOperationException("All players inactive.");
            }

            _button = next;
            _index = next;
        }

        private int NextActive(int start)
        {
            int index = start;
            int count = _players.Count;

            for (int i = 0; i < count; i++)
            {
                index = (index + 1) % count;
                if (_players[index].CanAct)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
