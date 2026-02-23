using System;
using Xunit;

namespace Holdem.Engine.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void TestContribute_ValidAmount()
        {
            int stack = 100;
            int amount = 50;
            var player = new Player("p", stack);
            int contribution = player.Contribute(amount);

            Assert.Equal(amount, contribution);
            Assert.Equal(stack - contribution, player.Stack);
        }

        [Fact]
        public void TestContribute_InvalidAmount()
        {
            var player = new Player("p", 100);

            Assert.Throws<ArgumentException>(() => player.Contribute(player.Stack + 1));
        }

        [Fact]
        public void TestDealCard_MoreThanTwo()
        {
            var player = new Player("a", 100);
            player.DealCard(new());
            player.DealCard(new());

            Assert.Throws<InvalidOperationException>(() => player.DealCard(new()));
        }
    }
}
