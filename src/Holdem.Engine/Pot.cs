using System.Collections.Generic;
using System.Linq;
using Holdem.Common.Extensions;
using Holdem.Core;

namespace Holdem.Engine
{
    public record Contestant(string PlayerId, IEnumerable<Card> Hand, PokerHandRanking Ranking);

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
            var pot = new Pot();
            pot.Merge(left);
            pot.Merge(right);
            return pot;
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

        public List<PokerEvent> Award(IEnumerable<Contestant> contestants)
        {
            if (contestants.Count() == 1)
            {
                var e = new PotAwardedEvent(contestants.Single().PlayerId, Total);

                return [e];
            }

            var events = new List<PokerEvent>();

            foreach (var sidePot in BuildSidePots())
            {
                var winners = contestants.Where(sidePot.IsEligible).ManyMaxBy(c => c.Ranking);

                int count = winners.Count;
                int share = sidePot.Amount / count;
                int oddChips = sidePot.Amount % count;
                var awarded = new int[count];

                for (int i = 0; i < count; i++)
                    awarded[i] += share;

                for (int i = 0; i < oddChips; i++)
                    awarded[i] += 1;

                for (int i = 0; i < count; i++)
                    events.Add(new PotAwardedEvent(winners[i].PlayerId, awarded[i]));
            }

            return events;
        }

        private List<SidePot> BuildSidePots()
        {
            var sidePots = new List<SidePot>();

            if (_contributions.Count == 0)
                return sidePots;

            var remaining = new Dictionary<string, int>(_contributions);

            while (remaining.Count > 0)
            {
                var contestants = remaining.Keys;
                int contribution = remaining.Values.Min();
                int amount = contribution * contestants.Count;

                sidePots.Add(new(amount, contestants));

                foreach (var player in remaining.Keys.ToList())
                {
                    remaining[player] -= contribution;
                    if (remaining[player] <= 0)
                    {
                        remaining.Remove(player);
                    }
                }
            }

            return sidePots;
        }

        private class SidePot(int amount, IEnumerable<string> eligible)
        {
            public int Amount { get; } = amount;

            private readonly IEnumerable<string> _eligible = [.. eligible];

            public bool IsEligible(Contestant c) => _eligible.Contains(c.PlayerId);
        }
    }
}
