using System;
using System.Collections.Generic;
using System.Linq;

namespace Holdem.Engine
{
    public class FixedLimitBettingRound : BettingRound
    {
        private readonly HashSet<string> _pending = [];
        private readonly PokerTable _table;
        private readonly Street _street;
        private readonly int _minBet;
        private readonly Pot _pot;

        private int _betSize;
        private int _maxRaises;
        private int _blindsPosted;
        private int _blind;

        public FixedLimitBettingRound(PokerTable table, Street street, int smallBet, int maxRaises)
        {
            _pending.UnionWith(table.AllActiveWithStack.Select(p => p.Id));

            _table = table;
            _street = street;

            int minBet = smallBet;
            if (street >= Street.Turn)
                minBet *= 2;

            _minBet = minBet;
            _maxRaises = maxRaises;

            _betSize = 0;
            _blindsPosted = 0;
            _blind = minBet / 2;

            _pot = new();
        }

        public Pot Pot => _pot;
        public int BetSize => _betSize;
        public bool Complete => _pending.Count == 0;

        public override BettingRoundResult Apply(string playerId, PlayerAction action)
        {
            if (ValidateTurn(playerId) == false)
            {
                return BettingRoundResult.Failed(new OutOfTurnEvent(playerId));
            }

            if (_street == Street.Preflop && _blindsPosted < 2)
            {
                return ApplyBlind(action);
            }

            return ApplyAction(action);
        }

        private BettingRoundResult ApplyBlind(PlayerAction action)
        {
            var player = _table.Current;
            var playerId = player.Id;

            if (ValidateBlind(player.Stack, action) == false)
            {
                return BettingRoundResult.Failed(new BlindErrorEvent(playerId, action, _blind));
            }

            int amount = action.Amount;
            _pot.Add(playerId, player.Contribute(amount));

            _betSize = _blind;
            _blindsPosted++;
            _blind *= 2;

            _pending.Remove(playerId);

            return new(new BlindPostedEvent(playerId, amount));
        }

        public int ToCall(Player player) => _betSize - _pot.GetContribution(player.Id);

        public int ToRaise(Player player) => ToCall(player) + _minBet;

        public bool CanRaise => _maxRaises > 0 || _table.IsHeadsUp;

        private BettingRoundResult ApplyAction(PlayerAction action)
        {
            var player = _table.Current;
            var playerId = player.Id;

            int toCall = ToCall(player);
            int toRaise = ToRaise(player);
            var context = new PlayerContext(toCall, toRaise, CanRaise);

            if (ValidateAction(player.Stack, action, context) == false)
            {
                return BettingRoundResult.Failed(new InvalidActionEvent(playerId, action, context));
            }

            int amount = action.Amount;
            PokerEvent e = null;

            switch (action.Type)
            {
                case PlayerActionType.Check:
                {
                    e = new PlayerCheckedEvent(playerId);
                    break;
                }

                case PlayerActionType.Fold:
                {
                    player.Active = false;
                    e = new PlayerFoldedEvent(playerId);
                    break;
                }

                case PlayerActionType.Call:
                {
                    _pot.Add(playerId, player.Contribute(amount));
                    e = new PlayerCalledEvent(playerId, amount);
                    break;
                }

                case PlayerActionType.Bet:
                {
                    _pot.Add(playerId, player.Contribute(amount));
                    e = new PlayerBetEvent(playerId, amount);

                    var isFullRaise = (amount - toCall) == _minBet;
                    if (isFullRaise)
                    {
                        var roundOpen = _betSize > 0;
                        _betSize += _minBet;

                        if (roundOpen && _maxRaises > 0)
                            _maxRaises--;

                        // Re-open betting, everyone acts again.
                        _pending.UnionWith(_table.AllActiveWithStack.Select(p => p.Id));
                    }
                    else
                    {
                        // Not a full raise, do not re-open.
                    }

                    break;
                }
            }

            _pending.Remove(playerId);

            return new(e);
        }

        private bool ValidateTurn(string playerId)
        {
            return _table.Current.Id == playerId && _pending.Contains(playerId);
        }

        private bool ValidateBlind(int playerStack, PlayerAction action)
        {
            var type = action.Type;
            int amount = action.Amount;
            return type == PlayerActionType.Bet && amount == int.Min(playerStack, _blind);
        }

        private static bool ValidateAction(
            int playerStack,
            PlayerAction action,
            PlayerContext context
        )
        {
            var type = action.Type;
            int amount = action.Amount;
            var (toCall, toRaise, canRaise) = context;

            switch (type)
            {
                case PlayerActionType.Fold:
                    return amount == 0;

                case PlayerActionType.Check:
                    return toCall == 0;

                case PlayerActionType.Call:
                    return amount == int.Min(playerStack, toCall);

                case PlayerActionType.Bet:
                    return canRaise && amount == int.Min(playerStack, toRaise);

                default:
                    throw new ArgumentException(type.ToString());
            }
        }
    }
}
