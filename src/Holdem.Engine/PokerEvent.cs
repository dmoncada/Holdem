using System;

// csharpier-ignore

namespace Holdem.Engine
{
    public abstract record PokerEvent(DateTime Timestamp);

    public record BoardCardsDealtEvent(Street Street, string Cards)
        : PokerEvent(DateTime.UtcNow);

    public record HoleCardsDealtEvent(string PlayerId, string Cards)
        : PokerEvent(DateTime.UtcNow);

    public record DealCardsCompletedEvent(Street Street)
        : PokerEvent(DateTime.UtcNow);

    public record BlindPostedEvent(string PlayerId, int Amount)
        : PokerEvent(DateTime.UtcNow);

    public record PlayerTurnStartedEvent(string PlayerId, PokerGameSnapshot Snapshot)
        : PokerEvent(DateTime.UtcNow);

    public record PlayerFoldedEvent(string PlayerId)
        : PokerEvent(DateTime.UtcNow);

    public record PlayerCheckedEvent(string PlayerId)
        : PokerEvent(DateTime.UtcNow);

    public record PlayerCalledEvent(string PlayerId, int Amount)
        : PokerEvent(DateTime.UtcNow);

    public record PlayerBetEvent(string PlayerId, int Amount)
        : PokerEvent(DateTime.UtcNow);

    public record HandShownEvent(string PlayerId, string BestHand)
        : PokerEvent(DateTime.UtcNow);

    public record BettingRoundStartedEvent()
        : PokerEvent(DateTime.UtcNow);

    public record BettingRoundCompletedEvent()
        : PokerEvent(DateTime.UtcNow);

    public record PotAwardedEvent(string PlayerId, int Amount)
        : PokerEvent(DateTime.UtcNow);

    public record ShowdownStartedEvent()
        : PokerEvent(DateTime.UtcNow);

    public record ShowdownCompletedEvent()
        : PokerEvent(DateTime.UtcNow);

    #region Failure Events

    public abstract record ErrorEvent()
        : PokerEvent(DateTime.UtcNow);

    public record RoundAlreadyCompleteEvent(string PlayerId)
        : ErrorEvent();

    public record OutOfTurnEvent(string PlayerId)
        : ErrorEvent();

    public record InvalidBlindEvent(string PlayerId, PlayerAction Action, int BlindAmout)
        : ErrorEvent();

    public record InvalidActionEvent(string PlayerId, PlayerAction Action, BettingContext Context)
        : ErrorEvent();

    #endregion Failure Events
}
