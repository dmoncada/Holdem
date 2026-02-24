using System.Collections.Generic;

namespace Holdem.Engine
{
    public class PokerGameSnapshot
    {
        public string PlayerId { get; init; }
        public int Stack { get; init; }
        public int SmallBlind { get; init; }
        public int BigBlind { get; init; }
        public int Pot { get; init; }
        public int ToCall { get; init; }
        public int ToRaise { get; init; }
        public bool CanRaise { get; init; }
        public Street Street { get; init; }
        public string HoleCards { get; init; }
        public string BoardCards { get; init; }
        public IReadOnlyList<CommitedAction> History { get; init; }
    }
}
