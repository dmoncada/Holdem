namespace Holdem.Engine
{
    public class NoLimitStructure : BettingStructure
    {
        private int _lastRaise = 0;
        public override int ToRaise => _lastRaise;

        public override bool CanRaise => true;

        public override bool ApplyRaise(int toCall, int amount)
        {
            int raise = amount - toCall;
            if (raise >= _lastRaise)
            {
                _lastRaise = raise;
                _betSize += raise;

                return true;
            }

            return false;
        }

        protected override bool ValidateRaise(int expected, int amount)
        {
            return amount >= expected;
        }
    }
}
