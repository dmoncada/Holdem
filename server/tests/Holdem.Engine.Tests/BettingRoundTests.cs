using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Holdem.Engine.PlayerAction;

namespace Holdem.Engine.Tests
{
    public class BettingRoundTests
    {
        private const int BigBlind = 4;
        private const int SmallBlind = BigBlind / 2;

        private static Player P(bool active = true) => new("p", 100) { Active = active };

        private static FixedLimitStructure S(PokerTable table, int minBet) => new(table, minBet, 3);

        private static bool ValidateBlindBet(PokerEvent e, string playerId, int amount) =>
            e is BlindPostedEvent b && (b.PlayerId, b.Amount) == (playerId, amount);

        [Fact]
        public void TestApplyAction_OutOfTurn()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);
            var wrongPlayer = players[1];

            var round = new BettingRound(table, Street.Preflop, BigBlind, S(table, BigBlind));

            var result = round.Apply(wrongPlayer.Id, Check());

            Assert.Single(result.Events);
            Assert.Equal(0, round.BetSize);
            Assert.Equal(0, round.Pot.Total);
            Assert.True(result.Events[0] is OutOfTurnEvent e && e.PlayerId == wrongPlayer.Id);
            Assert.False(result.Success);
        }

        [Fact]
        public void TestApplyAction_InvalidBlind()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);

            var round = new BettingRound(table, Street.Preflop, BigBlind, S(table, BigBlind));
            var events = new List<PokerEvent>();
            var success = false;

            PlayerAction[] actions = [Fold(), Check(), Call(1), Bet(1)];

            foreach (var action in actions)
            {
                var result = round.Apply(table.Current.Id, action);
                events.AddRange(result.Events);
                success |= result.Success;
                // Do not advance turn.
            }

            Assert.Equal(actions.Length, events.Count);
            Assert.Equal(0, round.BetSize);
            Assert.Equal(0, round.Pot.Total);
            Assert.True(events.All(e => e is InvalidBlindEvent b && b.PlayerId == players[0].Id));
            Assert.False(success);
        }

        [Fact]
        public void TestApplyAction_ValidBlind()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);

            var round = new BettingRound(table, Street.Preflop, BigBlind, S(table, BigBlind));
            var events = new List<PokerEvent>();
            var success = true;

            PlayerAction[] actions = [Bet(SmallBlind), Bet(BigBlind)];

            foreach (var action in actions)
            {
                var result = round.Apply(table.Current.Id, action);
                events.AddRange(result.Events);
                success &= result.Success;
                table.MoveNext();
            }

            Assert.Equal(actions.Length, events.Count);
            Assert.Equal(BigBlind, round.BetSize);
            Assert.Equal(BigBlind + SmallBlind, round.Pot.Total);
            Assert.True(ValidateBlindBet(events[0], players[0].Id, SmallBlind));
            Assert.True(ValidateBlindBet(events[1], players[1].Id, BigBlind));
            Assert.True(success);
        }

        [Fact]
        public void TestApplyAction_ActionAfterRoundComplete()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);

            var round = new BettingRound(table, Street.Flop, 4, S(table, 4));
            var events = new List<PokerEvent>();
            var success = true;

            PlayerAction[] before = [Bet(4), Call(4), Call(4)];
            PlayerAction[] after = [Fold(), Check()];

            foreach (var action in before.Concat(after))
            {
                var result = round.Apply(table.Current.Id, action);
                events.AddRange(result.Events);
                success &= result.Success;
                table.MoveNext();
            }

            Assert.Equal(before.Length, events.Count(e => e is not ErrorEvent));
            Assert.Equal(after.Length, events.Count(e => e is RoundAlreadyCompleteEvent));
            Assert.True(round.Complete);
            Assert.False(success);
        }

        [Fact]
        public void TestApplyAction_ValidFixedLimitRound()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);

            var round = new BettingRound(table, Street.Flop, 4, S(table, 4));
            var events = new List<PokerEvent>();
            var success = true;

            PlayerAction[] actions =
            [
                Bet(4), // p1, $4
                Bet(8), // p2, $8
                Bet(12), // p3, $12
                Bet(12), // p1, $16
                Call(8), // p2, $16
                Call(4), // p3, $16
            ];

            foreach (var action in actions)
            {
                var result = round.Apply(table.Current.Id, action);
                events.AddRange(result.Events);
                success &= result.Success;
                table.MoveNext();
            }

            int betCount = actions.Count(a => a.Type == PlayerActionType.Bet);
            int callCount = actions.Count(a => a.Type == PlayerActionType.Call);
            int total = actions.Sum(a => a.Amount);

            Assert.Equal(actions.Length, events.Count);
            Assert.DoesNotContain(events, e => e is ErrorEvent);
            Assert.Equal(betCount, events.Count(e => e is PlayerBetEvent));
            Assert.Equal(callCount, events.Count(e => e is PlayerCalledEvent));
            Assert.Equal(total, round.Pot.Total);
            Assert.False(round.CanRaise);
            Assert.True(round.Complete);
            Assert.True(success);
        }

        [Fact]
        public void TestApplyAction_ValidNoLimitRound()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);

            var round = new BettingRound(table, Street.Flop, 4, new NoLimitStructure());
            var events = new List<PokerEvent>();
            var success = true;

            PlayerAction[] actions =
            [
                Bet(2), // p1, $4
                Bet(8), // p2, $8
                Bet(14), // p3, $14
                Call(12), // p1, $14
                Call(6), // p2, $14
            ];

            foreach (var action in actions)
            {
                var result = round.Apply(table.Current.Id, action);
                events.AddRange(result.Events);
                success &= result.Success;
                table.MoveNext();
            }

            int betCount = actions.Count(a => a.Type == PlayerActionType.Bet);
            int callCount = actions.Count(a => a.Type == PlayerActionType.Call);
            int total = actions.Sum(a => a.Amount);

            Assert.Equal(actions.Length, events.Count);
            Assert.DoesNotContain(events, e => e is ErrorEvent);
            Assert.Equal(betCount, events.Count(e => e is PlayerBetEvent));
            Assert.Equal(callCount, events.Count(e => e is PlayerCalledEvent));
            Assert.Equal(total, round.Pot.Total);
            Assert.True(round.CanRaise); // No limit, can always raise.
            Assert.True(round.Complete);
            Assert.True(success);
        }
    }
}
