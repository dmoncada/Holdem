namespace Holdem.Engine
{
    public readonly struct PlayerAction(PlayerActionType type, int amount)
    {
        public static PlayerAction Fold() => new(PlayerActionType.Fold, 0);

        public static PlayerAction Check() => new(PlayerActionType.Check, 0);

        public static PlayerAction Call(int amount) => new(PlayerActionType.Call, amount);

        public static PlayerAction Bet(int amount) => new(PlayerActionType.Bet, amount);

        public PlayerActionType Type { get; } = type;
        public int Amount { get; } = amount;
    }
}
