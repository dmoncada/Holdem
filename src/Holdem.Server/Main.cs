using Holdem.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Holdem.Server
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddGrpc();
            builder.Services.AddSingleton<PokerSessionManager>();

            var app = builder.Build();

            app.MapGrpcService<PokerService>();
            app.MapGet("/", () => "Poker server running.");
            app.Run();
        }
    }
}
