using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Holdem.Common.Extensions;
using Holdem.Core;
using Stateless;
using Stateless.Graph;

namespace Holdem.Engine
{
    public record CommitedAction(Street Street, string PlayerId, PlayerAction Action);

    public class PokerStateMachine
    {
        // See: https://pokergamedevelopers.com/poker-game-logic-uml-statecharts-guide/
        private enum State
        {
            WaitingForPlayers = 0,
            Deal, // <- PreFlop.
            PreFlopBetting,
            DealFlop,
            FlopBetting,
            DealTurn,
            TurnBetting,
            DealRiver,
            RiverBetting,
            Showdown,
            Cleanup,
        }

        private enum Trigger
        {
            NextState,
            ToShowdown,
        }

        private static readonly HashSet<State> DealStates =
        [
            State.Deal,
            State.DealFlop,
            State.DealRiver,
            State.DealTurn,
        ];

        private static readonly HashSet<State> BettingStates =
        [
            State.PreFlopBetting,
            State.FlopBetting,
            State.TurnBetting,
            State.RiverBetting,
        ];

        private const int MaxRaises = 3;

        private readonly StateMachine<State, Trigger> _machine = null;
        private readonly List<CommitedAction> _bettingHistory = [];
        private readonly PokerTable _table = null;
        private readonly List<Card> _board = [];
        private readonly Deck _deck = null;
        private readonly int _smallBet;

        private BettingRound _round = null;
        private Street? _street = null;
        private Pot _pot = null;

        public PokerStateMachine(int smallBet, Deck deck = null)
        {
            _machine = BuildStateMachine();

            _deck = deck ?? new();

            _smallBet = smallBet; // The smallest betting unit.

            _table = new();

            Reset();
        }

        public PokerTable Table => _table;
        public bool IsDealing => DealStates.Contains(CurrentState);
        public bool IsBetting => BettingStates.Contains(CurrentState);
        public bool IsReady => CurrentState == State.WaitingForPlayers && NumActive > 1;

        private State CurrentState => _machine?.State ?? State.WaitingForPlayers;
        private int NumActive => _table.AllActiveWithStack.Count();

        private StateMachine<State, Trigger> BuildStateMachine()
        {
            var machine = new StateMachine<State, Trigger>(State.WaitingForPlayers);

            machine
                .Configure(State.WaitingForPlayers)
                .OnEntry(Reset)
                .Permit(Trigger.NextState, State.Deal);

            // PreFlop.
            machine
                .Configure(State.Deal)
                .OnEntryAsync(OnDealCardsEntryAsync)
                .OnExitAsync(OnDealCardsExitAsync)
                .Permit(Trigger.NextState, State.PreFlopBetting);

            machine
                .Configure(State.PreFlopBetting)
                .OnEntryAsync(OnBettingRoundEntryAsync)
                .OnExitAsync(OnBettingRoundExitAsync)
                .PermitIf(Trigger.NextState, State.DealFlop, () => NumActive > 1)
                .PermitIf(Trigger.ToShowdown, State.Showdown, () => NumActive <= 1);

            // Flop.
            machine
                .Configure(State.DealFlop)
                .OnEntryAsync(OnDealCardsEntryAsync)
                .OnExitAsync(OnDealCardsExitAsync)
                .Permit(Trigger.NextState, State.FlopBetting);

            machine
                .Configure(State.FlopBetting)
                .OnEntryAsync(OnBettingRoundEntryAsync)
                .OnExitAsync(OnBettingRoundExitAsync)
                .PermitIf(Trigger.NextState, State.DealTurn, () => NumActive > 1)
                .PermitIf(Trigger.ToShowdown, State.Showdown, () => NumActive <= 1);

            // Turn.
            machine
                .Configure(State.DealTurn)
                .OnEntryAsync(OnDealCardsEntryAsync)
                .OnExitAsync(OnDealCardsExitAsync)
                .Permit(Trigger.NextState, State.TurnBetting);

            machine
                .Configure(State.TurnBetting)
                .OnEntryAsync(OnBettingRoundEntryAsync)
                .OnExitAsync(OnBettingRoundExitAsync)
                .PermitIf(Trigger.NextState, State.DealRiver, () => NumActive > 1)
                .PermitIf(Trigger.ToShowdown, State.Showdown, () => NumActive <= 1);

            // River.
            machine
                .Configure(State.DealRiver)
                .OnEntryAsync(OnDealCardsEntryAsync)
                .OnExitAsync(OnDealCardsExitAsync)
                .Permit(Trigger.NextState, State.RiverBetting);

            machine
                .Configure(State.RiverBetting)
                .OnEntryAsync(OnBettingRoundEntryAsync)
                .OnExitAsync(OnBettingRoundExitAsync)
                .Permit(Trigger.NextState, State.Showdown);

            // Showdown.
            machine
                .Configure(State.Showdown)
                .OnEntryAsync(OnShowdownEntryAsync)
                .OnExitAsync(OnShowdownExitAsync)
                .Permit(Trigger.NextState, State.Cleanup);

            machine
                .Configure(State.Cleanup)
                .OnEntryAsync(CleanupAsync)
                .Permit(Trigger.NextState, State.WaitingForPlayers);

            return machine;
        }

        public async Task StartAsync()
        {
            if (IsReady == false)
                throw new InvalidOperationException("Cannot start, not ready.");

            await AdvanceAsync();
        }

        private async Task AdvanceAsync()
        {
            if (IsBetting && _round.Complete == false)
                throw new InvalidOperationException("Cannot advance, betting round ongoing.");

            var trigger = Trigger.NextState;

            if (IsBetting && NumActive <= 1)
                trigger = Trigger.ToShowdown;

            await _machine.FireAsync(trigger);
        }

        private void Reset()
        {
            _table.Reset();
            _table.AllActiveWithStack.ForEach(p => p.Reset());

            _deck.Reset();
            _deck.Shuffle();

            _bettingHistory.Clear();
            _board.Clear();

            _round = null;
            _street = null;
            _pot = new();
        }

        private async Task OnDealCardsEntryAsync()
        {
            var events = new List<PokerEvent>();

            _street = _street?.Next() ?? Street.Preflop;

            var street = _street.Value;
            switch (street)
            {
                case Street.Preflop:
                    events.AddRange(DealCards(_table.AllActiveWithStack.Rotate(1)));
                    break;

                case Street.Flop:
                    _deck.Draw(); // <- Burn.
                    _board.AddRange(_deck.Draw(3));
                    events.Add(new BoardCardsDealtEvent(street, _board.AsString()));
                    break;

                case Street.Turn:
                case Street.River:
                    _deck.Draw(); // <- Burn.
                    _board.Add(_deck.Draw());
                    events.Add(new BoardCardsDealtEvent(street, _board.AsString()));
                    break;

                default:
                    throw new InvalidEnumArgumentException(nameof(_street));
            }

            foreach (var e in events)
            {
                await WriteAsync(e); // Emit events.
            }

            await AdvanceAsync();
        }

        private IEnumerable<HoleCardsDealtEvent> DealCards(IEnumerable<Player> players)
        {
            for (int i = 0; i < 2; i++)
            {
                foreach (var player in players)
                {
                    var card = _deck.Draw();

                    player.TakeCard(card);

                    yield return new HoleCardsDealtEvent(player.Id, card.ToString());
                }
            }
        }

        private async Task OnDealCardsExitAsync()
        {
            await WriteAsync(new DealCardsCompletedEvent(_street.Value));
        }

        private async Task OnBettingRoundEntryAsync()
        {
            _table.Reset();

            AdvanceCurrentPlayer();

            var structure = new FixedLimitStructure(_table, _smallBet, MaxRaises);

            _round = new BettingRound(_table, _street.Value, _smallBet, structure);

            await WriteAsync(new BettingRoundStartedEvent());

            await SignalCurrentPlayerAsync();
        }

        private void AdvanceCurrentPlayer()
        {
            if (_street == Street.Preflop && _table.IsHeadsUp)
            {
                // In a "heads-up" preflop betting round, the blinds are reversed in order.
                // I.e. the button posts the small blind, and the opponent posts the big blind.
                // This means that the button should be the first player to act.
            }
            else
            {
                _table.MoveNext(); // Player to the left of button acts first.
            }
        }

        public async Task ApplyActionAsync(string playerId, PlayerAction action)
        {
            if (IsBetting == false)
            {
                throw new InvalidOperationException("Not in betting state.");
            }

            var result = _round.Apply(playerId, action);

            if (result.Success)
            {
                _bettingHistory.Add(new CommitedAction(_street.Value, playerId, action));

                _table.MoveNext(); // No errors, advance turn.
            }

            foreach (var e in result.Events)
            {
                await WriteAsync(e); // Emit events.
            }

            if (_round.Complete)
            {
                _pot += _round.Pot;

                await AdvanceAsync();
            }
            else // Signal the player to act.
            {
                await SignalCurrentPlayerAsync();
            }
        }

        private async Task SignalCurrentPlayerAsync()
        {
            await WriteAsync(new PlayerTurnStartedEvent(_table.Current.Id, BuildSnapshot()));
        }

        private PokerGameSnapshot BuildSnapshot()
        {
            var player = _table.Current;
            var stakes = _pot.Total + _round.Pot.Total;
            var (toCall, toRaise, canRaise) = _round.GetContext(player);

            return new()
            {
                PlayerId = player.Id,
                Stack = player.Stack,
                SmallBlind = _smallBet / 2,
                BigBlind = _smallBet,
                Pot = stakes,
                ToCall = toCall,
                ToRaise = toRaise,
                CanRaise = canRaise,
                Street = _street.Value,
                HoleCards = player.Hole.AsString(),
                BoardCards = _board.AsString(),
                History = _bettingHistory,
            };
        }

        private async Task OnBettingRoundExitAsync()
        {
            await WriteAsync(new BettingRoundCompletedEvent());
        }

        private async Task OnShowdownEntryAsync()
        {
            await WriteAsync(new ShowdownStartedEvent());

            await EnsureBoardCompleteAsync();

            var active = _table.AllActive;

            var trips = active.Select(player =>
            {
                var p = player.Id;
                var c = player.Hole.Concat(_board);
                var h = PokerHandRanking.BestHand(c);
                var r = PokerHandRanking.FromHand(h);
                return new Contestant(p, h, r);
            });

            if (trips.Count() > 1) // Show cards.
            {
                foreach (var (p, h, _) in trips)
                {
                    await WriteAsync(new HandShownEvent(p, h.OrderDescending().AsString()));
                }
            }

            var events = _pot.Award(trips);

            foreach (var e in events)
            {
                if (e is PotAwardedEvent p)
                {
                    var player = active.Single(a => a.Id == p.PlayerId);

                    player.Stack += p.Amount;
                }

                await WriteAsync(e);
            }

            await AdvanceAsync();
        }

        private async Task EnsureBoardCompleteAsync()
        {
            int count = _board.Count;
            if (count < 5)
            {
                _board.AddRange(_deck.Draw(5 - count));

                await WriteAsync(new BoardCardsDealtEvent(_street.Value, _board.AsString()));
            }
        }

        private async Task OnShowdownExitAsync()
        {
            await WriteAsync(new ShowdownCompletedEvent());
        }

        private async Task CleanupAsync()
        {
            _table.MoveButton(); // Move button clockwise to the left.

            // _channel.Writer.Complete(); // <- Correct?

            await AdvanceAsync();
        }

        private async Task WriteAsync(PokerEvent e)
        {
            await _channel.Writer.WriteAsync(e);
        }

        public ChannelReader<PokerEvent> Events => _channel.Reader;

        private readonly Channel<PokerEvent> _channel = Channel.CreateUnbounded<PokerEvent>();

        public string ToUmlDot() => UmlDotGraph.Format(_machine.GetInfo());

        public string ToMermaid() => MermaidGraph.Format(_machine.GetInfo());
    }
}
