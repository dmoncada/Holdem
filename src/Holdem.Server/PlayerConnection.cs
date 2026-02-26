using Grpc.Core;
using Holdem.Proto;

namespace Holdem.Server
{
    public class PlayerConnection(
        string sessionId,
        string playerId,
        string playerName,
        IServerStreamWriter<ConnectResponse> responseStream
    )
    {
        public string SessionId { get; } = sessionId;
        public string PlayerId { get; } = playerId;
        public string PlayerName { get; } = playerName;
        public IServerStreamWriter<ConnectResponse> ResponseStream { get; } = responseStream;
    }
}
