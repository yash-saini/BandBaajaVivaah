using BandBaajaVivaah.Grpc;
using Grpc.Core;
using Grpc.Net.Client;
using System.Net.Http;
using System.Windows;
using System.Windows.Threading;

namespace BandBaajaVivaah.WPF.Services
{
    public class GuestUpdateService : IAsyncDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly Grpc.GuestUpdateService.GuestUpdateServiceClient _client;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<GuestUpdateEvent> OnGuestUpdate;

        public GuestUpdateService(string serverUrl)
        {
            _channel = GrpcChannel.ForAddress(serverUrl);
            _client = new Grpc.GuestUpdateService.GuestUpdateServiceClient(_channel);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task SubscribeToWeddingUpdates(int weddingId)
        {
            try
            {
                var request = new GuestUpdateSubscriptionRequest { WeddingId = weddingId };
                using var call = _client.SubscribeToGuestUpdates(request);

                Console.WriteLine($"GuestUpdateService: Subscribed to updates for wedding {weddingId}");

                while (await call.ResponseStream.MoveNext(_cancellationTokenSource.Token))
                {
                    var update = call.ResponseStream.Current;
                    OnGuestUpdate?.Invoke(this, update);
                    Console.WriteLine($"GuestUpdateService: Received update for wedding {weddingId}, guest {update.Guest.GuestId}, type {update.Type}");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine($"GuestUpdateService: Subscription cancelled for wedding {weddingId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GuestUpdateService: Error in subscription for wedding {weddingId}: {ex.Message}");
            }
        }

        public void Unsubscribe()
        {
            _cancellationTokenSource.Cancel();
        }


        public async ValueTask DisposeAsync()
        {
            //Unsubscribe();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            if (_channel != null)
            {
                await _channel.ShutdownAsync();
                _channel.Dispose();
            }
        }
    }
}