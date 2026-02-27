using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Holdem.Core;
using Xunit;

namespace Holdem.Engine.Tests
{
    public class MockDeck(IEnumerable<Card> cards) : Deck(cards)
    {
        public override void Shuffle() { } // No-op.
    }

    public class PokerStateMachineTests
    {
        private static bool ValidateTurn(PokerEvent e, string playerId) =>
            e is PlayerTurnStartedEvent p && p.PlayerId == playerId;

        private static bool ValidateWin(PokerEvent e, string playerId, int amount) =>
            e is PotAwardedEvent p && (p.PlayerId, p.Amount) == (playerId, amount);

        private const int BigBlind = 4;
        private const int SmallBlind = BigBlind / 2;

        [Fact]
        public async Task TestGame_ValidPlayAsync()
        {
            var cards = "Kh Jd Qh 10c Jh 10s 2h 9s 6c";
            var deck = new MockDeck(cards.Split(' ').Select(Card.Parse));

            var p1 = new Player("P1", BigBlind * 2); // <- Button.
            var p2 = new Player("P2", BigBlind * 2);
            int wins = p1.Stack + p2.Stack;

            var game = new PokerStateMachine(BigBlind, deck);
            await game.AddPlayerAsync(p1);
            await game.AddPlayerAsync(p2);

            var events = new List<PokerEvent>();

            // "Heads-up" game, button acts first.

            await game.StartAsync();
            // Hole cards are dealt.
            await game.ApplyActionAsync(p1.Id, new(PlayerActionType.Bet, SmallBlind));
            await game.ApplyActionAsync(p2.Id, new(PlayerActionType.Bet, BigBlind));
            // Go all-in.
            await game.ApplyActionAsync(p1.Id, new(PlayerActionType.Bet, p1.Stack));
            await game.ApplyActionAsync(p2.Id, new(PlayerActionType.Call, p2.Stack));

            while (game.Events.TryRead(out var e))
            {
                events.Add(e);
            }

            Assert.Equal(1, events.Count(e => e is DealCardsCompletedEvent));
            Assert.Equal(1, events.Count(e => e is BettingRoundCompletedEvent));
            Assert.Equal(1, events.Count(e => e is ShowdownCompletedEvent));

            Assert.Equal(4, events.Count(e => e is HoleCardsDealtEvent));
            Assert.Equal(1, events.Count(e => e is BoardCardsDealtEvent));

            Assert.Equal(2, events.Count(e => ValidateTurn(e, p1.Id)));
            Assert.Equal(2, events.Count(e => ValidateTurn(e, p2.Id)));

            Assert.Equal(2, events.Count(e => e is BlindPostedEvent));
            Assert.Equal(1, events.Count(e => e is PlayerBetEvent));
            Assert.Equal(1, events.Count(e => e is PlayerCalledEvent));
            Assert.Equal(2, events.Count(e => e is HandShownEvent));

            // P2 wins with a straight: [Kh, Qh, Jh, 10s, 9s]
            Assert.Contains(events, e => ValidateWin(e, p2.Id, wins));
            Assert.Equal(wins, p2.Stack);
            Assert.Equal(0, p1.Stack);
        }
    }
}
