using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Holdem.Proto;

namespace Holdem.Console
{
    public class EntryPoint
    {
        public static async Task Main()
        {
            const string address = "http://localhost:5192";

            System.Console.Write("Session ID: ");
            var sessionId = System.Console.ReadLine();

            System.Console.Write("Player Name: ");
            var playerName = System.Console.ReadLine();

            using var channel = GrpcChannel.ForAddress(address);
            var client = new PokerService.PokerServiceClient(channel);
            using var call = client.Connect();

            await call.RequestStream.WriteAsync(
                new ConnectRequest
                {
                    Join = new JoinSession { SessionId = sessionId, PlayerName = playerName },
                }
            );

            System.Console.WriteLine("Connected.");

            var readTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    if (response.PayloadCase == ConnectResponse.PayloadOneofCase.Event)
                    {
                        System.Console.WriteLine(
                            $"[EVENT] {response.Event.Type} | {response.Event.Data}"
                        );
                    }
                    else if (response.PayloadCase == ConnectResponse.PayloadOneofCase.Error)
                    {
                        System.Console.WriteLine($"[ERROR] {response.Error.Message}");
                    }
                }
            });

            await call.RequestStream.CompleteAsync();

            await readTask;

            System.Console.WriteLine("Disconnected.");
        }
    }
}
