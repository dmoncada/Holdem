using System;
using System.Threading.Tasks;
using Grpc.Core;
using Holdem.Proto;
using static Holdem.Common.Utils;
using static Holdem.Proto.PokerService;

namespace Holdem.Server.Services
{
    public class PokerService(PokerSessionManager manager) : PokerServiceBase
    {
        private readonly Status NotJoined = new(StatusCode.FailedPrecondition, "Not joined.");

        private readonly PokerSessionManager _manager = manager;

        public override async Task Connect(
            IAsyncStreamReader<ConnectRequest> requestStream,
            IServerStreamWriter<ConnectResponse> responseStream,
            ServerCallContext context
        )
        {
            PlayerConnection player = null;
            PokerSession session = null;

            try
            {
                await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    switch (request.PayloadCase)
                    {
                        case ConnectRequest.PayloadOneofCase.Join:
                        {
                            var join = request.Join;
                            var sessionId = join.SessionId;
                            var playerName = join.PlayerName;
                            var playerId = ShortGuid();

                            player = new PlayerConnection(
                                sessionId,
                                playerId,
                                playerName,
                                responseStream
                            );

                            session = _manager.GetOrCreate(sessionId);
                            await session.AddPlayerAsync(player);
                            break;
                        }

                        case ConnectRequest.PayloadOneofCase.Action:
                        {
                            if (player == null)
                                throw new RpcException(NotJoined);

                            await session.ApplyActionAsync(player.PlayerId, request.Action);
                            break;
                        }

                        case ConnectRequest.PayloadOneofCase.Leave:
                        {
                            if (player == null)
                                throw new RpcException(NotJoined);

                            await session.RemovePlayerAsync(player.PlayerId);
                            return; // <- Break out entirely.
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    // Client disconnected.
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (session != null && player != null)
                {
                    await session.RemovePlayerAsync(player.PlayerId);
                }
            }
        }
    }
}
