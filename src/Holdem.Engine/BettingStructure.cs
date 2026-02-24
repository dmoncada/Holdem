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
        protected abstract bool ValidateRaise(int amount, int expected);

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
                    return amount == int.Min(playerStack, toCall);

                case PlayerActionType.Bet:
                    return ValidateRaise(amount, int.Min(playerStack, toRaise));

                default:
                    throw new InvalidEnumArgumentException(type.ToString());
            }
        }
    }
}
