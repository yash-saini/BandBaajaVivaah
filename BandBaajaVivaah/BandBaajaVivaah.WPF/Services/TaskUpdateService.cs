using BandBaajaVivaah.Grpc;
using Grpc.Core;
using Grpc.Net.Client;

namespace BandBaajaVivaah.WPF.Services
{
    public class TaskUpdateService : IAsyncDisposable
    {
        private readonly string _serverUrl;
        private GrpcChannel _channel;
        private Grpc.TaskUpdateService.TaskUpdateServiceClient _client;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<TaskUpdateEvent> OnTasksUpdate;

        public TaskUpdateService(string serverUrl)
        {
            _serverUrl = serverUrl;
            _channel = GrpcChannel.ForAddress(_serverUrl);
            _client = new Grpc.TaskUpdateService.TaskUpdateServiceClient(_channel);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task SubscribeToWeddingUpdates(int weddingId)
        {
            try
            {
                var request = new TaskUpdateSubscriptionRequest { WeddingId = weddingId };
                using var call = _client.SubscribeToTaskUpdates(request);

                Console.WriteLine($"TaskUpdateService: Subscribed to updates for wedding {weddingId}");

                while (await call.ResponseStream.MoveNext(_cancellationTokenSource.Token))
                {
                    var update = call.ResponseStream.Current;
                    OnTasksUpdate?.Invoke(this, update);
                    Console.WriteLine($"TaskUpdateService: Received update for wedding {weddingId}, task {update.Task.TaskId}, type {update.Type}");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine($"TaskUpdateService: Subscription cancelled for wedding {weddingId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TaskUpdateService: Error in subscription for wedding {weddingId}: {ex.Message}");
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

            Console.WriteLine("TaskUpdateService: Disposed");
        }
    }
}