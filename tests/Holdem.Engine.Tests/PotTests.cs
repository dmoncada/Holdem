using System;
using System.Collections.Generic;
using System.Linq;
using Holdem.Core;
using Xunit;
using static Holdem.Common.Extensions.EnumerableExtensions;
using static Holdem.Core.PokerHandRanking;
using Ranking = Holdem.Core.PokerHandRanking;

namespace Holdem.Engine.Tests
{
    public class PotTests
    {
        private const string Hi = "As Ks Qs Js 10s";
        private const string Mid = "10s 9h 8d 7c 6s";
        private const string Lo = "10s 8h 6d 4c 2s";

        private static string P() => Guid.NewGuid().ToString();

        private static IEnumerable<Card> ToHand(string s) => s.Split(' ').Select(Card.Parse);

        private static Contestant ToContestant((string p, IEnumerable<Card> h, Ranking r) x) =>
            new(x.p, x.h, x.r);

        private static bool Validate(PokerEvent e, string winnerId, int amount) =>
            e is PotAwardedEvent p && (p.PlayerId, p.Amount) == (winnerId, amount);

        [Fact]
        public void Test_SingleWinner()
        {
            string[] names = [P(), P(), P()];
            string[] cards = [Hi, Lo, Lo];

            var hands = cards.Select(ToHand);
            var ranks = hands.Select(FromHand);
            var trips = Zip(names, hands, ranks).Select(ToContestant);

            var pot = new Pot();
            pot.Add(names[0], 10);
            pot.Add(names[1], 10);
            pot.Add(names[2], 10);

            var events = pot.Award(trips);

            Assert.Contains(events, e => Validate(e, names[0], pot.Total));
        }

        [Fact]
        public void Test_MultipleWinners()
        {
            string[] names = [P(), P(), P()];
            string[] cards = [Hi, Hi, Lo];

            var hands = cards.Select(ToHand);
            var ranks = hands.Select(FromHand);
            var trips = Zip(names, hands, ranks).Select(ToContestant);

            var pot = new Pot();
            pot.Add(names[0], 11);
            pot.Add(names[1], 11);
            pot.Add(names[2], 11);

            var events = pot.Award(trips);

            // Odd chip goes to first player.
            Assert.Contains(events, e => Validate(e, names[0], pot.Total / 2 + 1));
            Assert.Contains(events, e => Validate(e, names[1], pot.Total / 2));
        }

        [Fact]
        public void Test_SidePot()
        {
            string[] names = [P(), P(), P()];
            string[] cards = [Mid, Hi, Lo];

            var hands = cards.Select(ToHand);
            var ranks = hands.Select(FromHand);
            var trips = Zip(names, hands, ranks).Select(ToContestant);

            var pot = new Pot();
            pot.Add(names[0], 30);
            pot.Add(names[1], 10);
            pot.Add(names[2], 30);

            var events = pot.Award(trips);

            // P2 wins (main) pot, $30
            // P1 wins (side) pot, $40

            Assert.Contains(events, e => e is PotAwardedEvent p && Validate(p, names[1], 30));
            Assert.Contains(events, e => e is PotAwardedEvent p && Validate(p, names[0], 40));
        }
    }
}
