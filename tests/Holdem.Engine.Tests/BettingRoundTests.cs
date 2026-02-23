using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Holdem.Engine.PlayerAction;

namespace Holdem.Engine.Tests
{
    public class BettingRoundTests
    {
        private static Player P(bool active = true) => new("p", 100) { Active = active };

        private static bool ValidateBlindBet(PokerEvent e, string playerId, int amount) =>
            e is BlindPostedEvent b && (b.PlayerId, b.Amount) == (playerId, amount);

        [Fact]
        public void TestApplyAction_ValidBlindPosting()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);

            int bigBlind = 4;
            int smallBlind = bigBlind / 2;

            var round = new FixedLimitBettingRound(table, Street.Preflop, bigBlind, 3);
            var events = new List<PokerEvent>();

            PlayerAction[] actions = [Bet(smallBlind), Bet(bigBlind)];

            foreach (var action in actions)
            {
                var result = round.Apply(table.Current.Id, action);
                events.AddRange(result.Events);
                table.MoveNext();
            }

            Assert.Equal(2, events.Count);
            Assert.Equal(bigBlind, round.BetSize);
            Assert.Equal(smallBlind + bigBlind, round.Pot.Total);
            Assert.True(ValidateBlindBet(events[0], players[0].Id, smallBlind));
            Assert.True(ValidateBlindBet(events[1], players[1].Id, bigBlind));
        }

        [Fact]
        public void TestApplyAction_InvalidBlindPosting()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);

            int bigBlind = 4;
            int smallBlind = bigBlind / 2;

            var round = new FixedLimitBettingRound(table, Street.Preflop, bigBlind, 3);
            var events = new List<PokerEvent>();

            PlayerAction[] actions = [Fold(), Check(), Call(1), Bet(1)];

            foreach (var action in actions)
            {
                var result = round.Apply(table.Current.Id, action);
                events.AddRange(result.Events);
            }

            Assert.Equal(4, events.Count);
            Assert.Equal(0, round.BetSize);
            Assert.Equal(0, round.Pot.Total);
            Assert.True(events.All(e => e is BlindErrorEvent b && b.PlayerId == players[0].Id));
        }
    }
}
