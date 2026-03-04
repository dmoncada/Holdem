using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Holdem.Engine;
using Holdem.Proto;

namespace Holdem.Server
{
    public class PokerSession
    {
        private readonly ConcurrentDictionary<string, PlayerConnection> _players = new();
        private readonly PokerStateMachine _engine = null;
        private readonly CancellationTokenSource _cts = new();

        public PokerSession(string sessionId)
        {
            Id = sessionId;

            _engine = new PokerStateMachine(smallBet: 100);

            _ = NotifyAsync();
        }

        public string Id { get; }
        public bool IsEmpty => _players.IsEmpty;

        public async Task AddPlayerAsync(PlayerConnection player)
        {
            if (_players.TryAdd(player.PlayerId, player) == false)
            {
                Console.Error.WriteLine("Player already connected.");
            }

            Console.WriteLine("Adding player: '{0}', ID: {1}", player.PlayerName, player.PlayerId);

            var joining = new Player(id: player.PlayerId, name: player.PlayerName, stack: 1_000);

            await _engine.AddPlayerAsync(joining);

            if (_engine.IsReady)
            {
                await _engine.StartAsync();
            }
        }

        public async Task RemovePlayerAsync(string playerId)
        {
            if (_players.TryRemove(playerId, out _) == false)
            {
                Console.Error.WriteLine("Player already disconnected.");
            }

            await _engine.RemovePlayerAsync(playerId);
        }

        public async Task ApplyActionAsync(string playerId, Proto.PlayerAction action)
        {
            await _engine.ApplyActionAsync(playerId, default); //action.ToEngine());
        }

        private async Task NotifyAsync()
        {
            await foreach (var e in _engine.Events.ReadAllAsync(_cts.Token))
            {
                var response = new ConnectResponse
                {
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),

                    Event = new Proto.PokerEvent
                    {
                        Type = e.GetType().Name,

                        Data = JsonSerializer.Serialize(e),
                    },
                };

                await NotifyManyAsync(response);
            }
        }

        private async Task NotifySingleAsync(ConnectResponse response, string playerId)
        {
            var player = _players.Values.Single(p => p.PlayerId == playerId);

            await NotifyPlayerAsync(response, player);
        }

        private async Task NotifyManyAsync(ConnectResponse response)
        {
            foreach (var player in _players.Values)
            {
                await NotifyPlayerAsync(response, player);
            }
        }

        private async Task NotifyPlayerAsync(ConnectResponse response, PlayerConnection player)
        {
            try
            {
                await player.ResponseStream.WriteAsync(response);
            }
            catch
            {
                // TODO(dmoncada): handle broken clients.
            }
        }
    }
}
