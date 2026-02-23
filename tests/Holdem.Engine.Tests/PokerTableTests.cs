using Xunit;

namespace Holdem.Engine.Tests
{
    public class PokerTableTests
    {
        private static Player P(bool active = true) => new("p", 100) { Active = active };

        [Fact]
        public void TestReset_ButtonResets()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players, button: 1);

            table.MoveNext();
            table.Reset();

            Assert.Equal(players[1], table.Current);
        }

        [Fact]
        public void TestMoveNext_IterationWraps()
        {
            Player[] players = [P(), P(), P()];

            var table = new PokerTable(players);
            Assert.Equal(players[0], table.Current);

            table.MoveNext();
            Assert.Equal(players[1], table.Current);

            table.MoveNext();
            Assert.Equal(players[2], table.Current);

            table.MoveNext();
            Assert.Equal(players[0], table.Current);
        }

        [Fact]
        public void TestMoveNext_FalseWhenAllInactive()
        {
            Player[] players = [P(false), P(false), P(false)];
            var table = new PokerTable(players);

            var result = table.MoveNext();

            Assert.False(result);
        }

        [Fact]
        public void TestMoveNext_InactiveSkipped()
        {
            var p1 = P();
            var p2 = P(false);
            var p3 = P();

            Player[] players = [p1, p2, p3];

            var table = new PokerTable(players, button: 0);
            Assert.Equal(p1, table.Current);

            table.MoveNext();
            Assert.Equal(p3, table.Current);

            table.MoveNext();
            Assert.Equal(p1, table.Current);
        }

        [Fact]
        public void TestMoveButton_NextActive()
        {
            Player[] players = [P(), P(), P()];
            var table = new PokerTable(players);

            table.MoveButton();
            Assert.Equal(players[1], table.Current);

            table.MoveButton();
            Assert.Equal(players[2], table.Current);
        }

        [Fact]
        public void TestMoveButton_InactiveSkipped()
        {
            var p1 = P();
            var p2 = P(false);
            var p3 = P();

            Player[] players = [p1, p2, p3];
            var table = new PokerTable(players);

            table.MoveButton();
            table.Reset();

            Assert.Equal(p3, table.Current);
        }

        [Fact]
        public void TestHeadsUp()
        {
            var p1 = P();
            var p2 = P();
            var p3 = P();

            Player[] players = [p1, p2, p3];
            var table = new PokerTable(players);

            Assert.False(table.IsHeadsUp);

            p2.Active = false;
            Assert.True(table.IsHeadsUp);
        }
    }
}
