using System.ComponentModel;

namespace Holdem.Engine
{
    public record BettingContext(int ToCall, int ToRaise, bool CanRaise);

    public abstract class BettingStructure
    {
        protected int _betSize = 0;
        public int BetSize => _betSize;

        public void SetSize(int betSize) => _betSize = betSize;

        public abstract int ToRaise { get; }
        public abstract bool CanRaise { get; }
        public abstract bool ApplyRaise(int toCall, int amount);
        protected abstract bool ValidateRaise(int expected, int amount);

        public bool ValidateAction(int toCall, int playerStack, PlayerAction action)
        {
            var type = action.Type;
            int amount = action.Amount;
            int toRaise = toCall + ToRaise;

            switch (type)
            {
                case PlayerActionType.Fold:
                    return amount == 0;

                case PlayerActionType.Check:
                    return toCall == 0;

                case PlayerActionType.Call:
                    return int.Min(playerStack, toCall) == amount;

                case PlayerActionType.Bet:
                    return CanRaise && ValidateRaise(int.Min(playerStack, toRaise), amount);

                default:
                    throw new InvalidEnumArgumentException(nameof(type));
            }
        }
    }
}
