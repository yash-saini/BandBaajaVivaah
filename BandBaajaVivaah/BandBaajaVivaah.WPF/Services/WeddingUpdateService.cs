using BandBaajaVivaah.Grpc;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BandBaajaVivaah.WPF.Services
{
    public class WeddingUpdateService : IAsyncDisposable
    {
        private readonly string _serverUrl;
        private GrpcChannel _channel;
        private Grpc.WeddingUpdateService.WeddingUpdateServiceClient _client;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _subscriptionTask;

        public event EventHandler<WeddingUpdateEvent> OnWeddingUpdate;

        public WeddingUpdateService(string serverUrl)
        {
            _serverUrl = serverUrl;
            _channel = GrpcChannel.ForAddress(_serverUrl);
            _client = new Grpc.WeddingUpdateService.WeddingUpdateServiceClient(_channel);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task SubscribeToUserWeddingUpdates(int userId)
        {
            try
            {
                var request = new WeddingUpdateSubscriptionRequest { UserId = userId };
                var call = _client.SubscribeToWeddingUpdates(request);

                Console.WriteLine($"WeddingUpdateService: Subscribed to updates for user {userId}");

                while (await call.ResponseStream.MoveNext(_cancellationTokenSource.Token))
                {
                    var update = call.ResponseStream.Current;
                    OnWeddingUpdate?.Invoke(this, update);
                    Console.WriteLine($"WeddingUpdateService: Received update for user {userId}, wedding {update.Wedding.WeddingId}, type {update.Type}");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine($"WeddingUpdateService: Subscription cancelled for user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WeddingUpdateService: Error in subscription for user {userId}: {ex.Message}");
            }
        }

        public void Unsubscribe()
        {
            _cancellationTokenSource.Cancel();
        }

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            if (_channel != null)
            {
                await _channel.ShutdownAsync();
                _channel.Dispose();
            }

            Console.WriteLine("WeddingUpdateService: Disposed");
        }
    }
}