using System.Collections.Generic;
using System.Linq;

namespace Holdem.Engine
{
    public class BettingRound
    {
        private readonly BettingStructure _structure = null;
        private readonly HashSet<string> _pending = [];
        private readonly PokerTable _table;
        private readonly Street _street;
        private readonly Pot _pot;

        private int _blindsPosted;
        private int _blind;

        public BettingRound(
            PokerTable table,
            Street street,
            int smallBet,
            BettingStructure structure
        )
        {
            _pending.UnionWith(table.AllActiveWithStack.Select(p => p.Id));

            _table = table;
            _street = street;
            _structure = structure;

            _blindsPosted = 0;
            _blind = smallBet / 2;

            _pot = new();
        }

        public int BetSize => _structure.BetSize;
        public bool CanRaise => _structure.CanRaise;
        public bool Complete => _pending.Count == 0;
        public Pot Pot => _pot;

        public BettingContext GetContext(Player player)
        {
            int toCall = _structure.BetSize - _pot.GetContribution(player.Id);
            int toRaise = _structure.ToRaise + toCall;
            var canRaise = _structure.CanRaise;
            return new(toCall, toRaise, canRaise);
        }

        public BettingRoundResult Apply(string playerId, PlayerAction action)
        {
            if (Complete)
            {
                return BettingRoundResult.Failed(new ActionAfterRoundCompleted(playerId));
            }

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
                return BettingRoundResult.Failed(new InvalidBlindEvent(playerId, action, _blind));
            }

            int amount = action.Amount;
            _pot.Add(playerId, player.Contribute(amount));

            _structure.SetSize(_blind); // <- A bit awkward...

            _blindsPosted++;
            _blind *= 2;

            _pending.Remove(playerId);

            return new(new BlindPostedEvent(playerId, amount));
        }

        private BettingRoundResult ApplyAction(PlayerAction action)
        {
            var player = _table.Current;
            var playerId = player.Id;

            var context = GetContext(player);

            if (_structure.ValidateAction(context.ToCall, player.Stack, action) == false)
            {
                return BettingRoundResult.Failed(new InvalidActionEvent(playerId, action, context));
            }

            int toCall = context.ToCall;
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

                    if (_structure.ApplyRaise(toCall, amount))
                    {
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
    }
}
