namespace Holdem.Engine
{
    public abstract class BettingRound
    {
        public abstract BettingRoundResult Apply(string playerId, PlayerAction action);
    }
}
