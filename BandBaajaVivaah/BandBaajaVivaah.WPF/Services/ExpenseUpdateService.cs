using BandBaajaVivaah.Grpc;
using Grpc.Core;
using Grpc.Net.Client;

namespace BandBaajaVivaah.WPF.Services
{
    public class ExpenseUpdateService : IAsyncDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly Grpc.ExpenseUpdateService.ExpenseUpdateServiceClient _client;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<ExpenseUpdateEvent> OnExpenseUpdate;

        public ExpenseUpdateService(string serverUrl)
        {
            _channel = GrpcChannel.ForAddress(serverUrl);
            _client = new Grpc.ExpenseUpdateService.ExpenseUpdateServiceClient(_channel);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task SubscribeToWeddingUpdates(int weddingId)
        {
            try
            {
                var request = new ExpenseUpdateSubscriptionRequest { WeddingId = weddingId };
                using var call = _client.SubscribeToExpenseUpdates(request);

                Console.WriteLine($"ExpenseUpdateService: Subscribed to updates for wedding {weddingId}");

                while (await call.ResponseStream.MoveNext(_cancellationTokenSource.Token))
                {
                    var update = call.ResponseStream.Current;
                    OnExpenseUpdate?.Invoke(this, update);
                    Console.WriteLine($"ExpenseUpdateService: Received update for wedding {weddingId}, expense {update.Expense.ExpenseId}, type {update.Type}");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine($"ExpenseUpdateService: Subscription cancelled for wedding {weddingId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExpenseUpdateService: Error in subscription for wedding {weddingId}: {ex.Message}");
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
