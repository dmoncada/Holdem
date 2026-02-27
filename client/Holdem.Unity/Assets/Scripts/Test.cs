using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using Holdem.Proto;
using UnityEngine;

public class Test : MonoBehaviour
{
    private GrpcChannel _channel;
    private PokerService.PokerServiceClient _client;
    private AsyncDuplexStreamingCall<ConnectRequest, ConnectResponse> _call;

    private CancellationTokenSource _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();

        _ = RunConnectionAsync(_cts.Token);
    }

    private async Task RunConnectionAsync(CancellationToken token)
    {
        try
        {
            var handler = new YetAnotherHttpHandler()
            {
                Http2Only = true,
                SkipCertificateVerification = true,
            };

            var options = new GrpcChannelOptions { HttpHandler = handler };
            _channel = GrpcChannel.ForAddress("http://localhost:5192", options);
            _client = new PokerService.PokerServiceClient(_channel);
            _call = _client.Connect(cancellationToken: token);

            _ = ReadLoopAsync(token);

            await _call.RequestStream.WriteAsync(
                new ConnectRequest
                {
                    Join = new JoinSession { SessionId = "123", PlayerName = "dmoncada" },
                }
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection failed: {ex}", this);
        }
    }

    private async Task ReadLoopAsync(CancellationToken token)
    {
        try
        {
            await foreach (var response in _call.ResponseStream.ReadAllAsync(token))
            {
                if (response.PayloadCase == ConnectResponse.PayloadOneofCase.Event)
                {
                    Debug.Log($"[EVENT] {response.Event.Type}");
                }
                else if (response.PayloadCase == ConnectResponse.PayloadOneofCase.Error)
                {
                    Debug.Log($"[ERROR] {response.Error.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Stream cancelled", this);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Read loop error: {ex}", this);
        }
    }

    private async void OnDestroy()
    {
        try
        {
            _cts?.Cancel();

            if (_call != null)
            {
                await _call.RequestStream.CompleteAsync();
            }

            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Shutdown error: {ex}", this);
        }
    }
}
