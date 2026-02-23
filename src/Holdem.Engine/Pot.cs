using System.Collections.Generic;
using System.Linq;

namespace Holdem.Engine
{
    public class Pot
    {
        private readonly Dictionary<string, int> _contributions = [];

        public int Total => _contributions.Values.Sum();

        public void Add(string playerId, int amount)
        {
            _contributions[playerId] = GetContribution(playerId) + amount;
        }

        public int GetContribution(string playerId)
        {
            return _contributions.GetValueOrDefault(playerId, 0);
        }

        public static Pot operator +(Pot left, Pot right)
        {
            return left.Merge(right);
        }

        public Pot Merge(Pot other)
        {
            foreach (var (player, amount) in other._contributions)
            {
                Add(player, amount);
            }

            return this;
        }

        public override string ToString()
        {
            return $"Pot: ${Total}";
        }
    }
}
