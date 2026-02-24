namespace Holdem.Engine
{
    public class FixedLimitStructure(PokerTable table, int minBet, int maxRaises) : BettingStructure
    {
        private readonly PokerTable _table = table;

        private int _minBet = minBet;
        private int _maxRaises = maxRaises;

        public override int ToRaise => _minBet;
        public override bool CanRaise => _maxRaises > 0 || _table.IsHeadsUp;

        public override bool ApplyRaise(int toCall, int amount)
        {
            var isFullRaise = (amount - toCall) == _minBet;
            if (isFullRaise)
            {
                var roundOpen = _betSize > 0;
                _betSize += _minBet;

                if (roundOpen)
                    _maxRaises--;

                return true;
            }

            return false;
        }

        protected override bool ValidateRaise(int amount, int expected)
        {
            return CanRaise && amount == expected;
        }
    }
}
