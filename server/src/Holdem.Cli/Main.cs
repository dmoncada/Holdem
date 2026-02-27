using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Holdem.Proto;

namespace Holdem.Cli
{
    public class EntryPoint
    {
        public static async Task Main()
        {
            const string address = "http://localhost:5192";

            Console.Write("Session ID: ");
            var sessionId = Console.ReadLine();

            Console.Write("Player Name: ");
            var playerName = Console.ReadLine();

            using var channel = GrpcChannel.ForAddress(address);
            var client = new PokerService.PokerServiceClient(channel);
            using var call = client.Connect();

            await call.RequestStream.WriteAsync(
                new ConnectRequest
                {
                    Join = new JoinSession { SessionId = sessionId, PlayerName = playerName },
                }
            );

            Console.WriteLine("Connected.");

            var readTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    if (response.PayloadCase == ConnectResponse.PayloadOneofCase.Event)
                    {
                        Console.WriteLine($"[EVENT] {response.Event.Type} | {response.Event.Data}");
                    }
                    else if (response.PayloadCase == ConnectResponse.PayloadOneofCase.Error)
                    {
                        Console.WriteLine($"[ERROR] {response.Error.Message}");
                    }
                }
            });

            await Task.Delay(TimeSpan.FromSeconds(3));

            await call.RequestStream.CompleteAsync();

            await readTask;

            Console.WriteLine("Disconnected.");
        }
    }
}
