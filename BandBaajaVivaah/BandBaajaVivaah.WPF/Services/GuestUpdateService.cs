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
            var request = new GuestUpdateSubscriptionRequest { WeddingId = weddingId };

            try
            {
                using var call = _client.SubscribeToGuestUpdates(request);

                while (await call.ResponseStream.MoveNext(_cancellationTokenSource.Token))
                {
                    var update = call.ResponseStream.Current;
                    OnGuestUpdate?.Invoke(this, update);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is RpcException)
            {
                Console.WriteLine($"Subscription ended: {ex.Message}");
            }
        }

        public void Unsubscribe()
        {
            _cancellationTokenSource.Cancel();
        }


        public async ValueTask DisposeAsync()
        {
            Unsubscribe();
            _cancellationTokenSource.Dispose();
            await _channel.ShutdownAsync();
        }
    }
}