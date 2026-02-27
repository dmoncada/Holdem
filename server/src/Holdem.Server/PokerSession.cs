using System;
using System.Collections.Concurrent;
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

            _ = BroadcastAsync();
        }

        public string Id { get; }
        public bool IsEmpty => _players.IsEmpty;

        public async Task AddPlayerAsync(PlayerConnection player)
        {
            if (_players.TryAdd(player.PlayerId, player) == false)
            {
                throw new InvalidOperationException("Player already connected.");
            }

            Console.WriteLine("Adding player: '{0}', ID: {1}", player.PlayerName, player.PlayerId);

            var joining = new Player(id: player.PlayerId, name: player.PlayerName, stack: 10_000);

            await _engine.AddPlayerAsync(joining);

            if (_engine.IsReady)
            {
                await _engine.StartAsync();
            }
        }

        public void RemovePlayer(string playerId)
        {
            _players.TryRemove(playerId, out _);

            // TODO(dmoncada): remove player from table.
        }

        public async Task ApplyActionAsync(string playerId, Proto.PlayerAction action)
        {
            await _engine.ApplyActionAsync(playerId, Map(action));
        }

        private Engine.PlayerAction Map(Proto.PlayerAction action)
        {
            throw new NotImplementedException(); // TODO(dmoncada)
        }

        private async Task BroadcastAsync()
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

                foreach (var player in _players.Values)
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
    }
}
