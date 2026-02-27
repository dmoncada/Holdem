using System.Collections.Generic;
using System.Linq;

namespace Holdem.Engine
{
    public class BettingRoundResult(List<PokerEvent> events)
    {
        public static BettingRoundResult Failed(params ErrorEvent[] e) =>
            new(e) { Success = false };

        public BettingRoundResult(params PokerEvent[] events)
            : this(events.ToList()) { }

        public List<PokerEvent> Events { get; } = events;
        public bool Success { get; init; } = true;
    }
}
