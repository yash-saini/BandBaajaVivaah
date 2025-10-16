using BandBaajaVivaah.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace BandBaajaVivaah.Services.GrpcServices
{
    public class WeddingUpdateGrpcService : Grpc.WeddingUpdateService.WeddingUpdateServiceBase
    {
        private readonly ILogger<WeddingUpdateGrpcService> _logger;
        private static readonly Dictionary<int, List<IServerStreamWriter<WeddingUpdateEvent>>> _subscribers = new();
        private static readonly object _lock = new();

        public WeddingUpdateGrpcService(ILogger<WeddingUpdateGrpcService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeToWeddingUpdates(
            WeddingUpdateSubscriptionRequest request,
            IServerStreamWriter<WeddingUpdateEvent> responseStream,
            ServerCallContext context)
        {
            var userId = request.UserId;

            // Add this client to the subscribers for this user
            AddSubscriber(userId, responseStream);

            try
            {
                // Keep the stream open until the client disconnects
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Client disconnected
                _logger.LogInformation($"Client disconnected from user {userId} wedding updates");
            }
            finally
            {
                // Remove the subscriber when they disconnect
                RemoveSubscriber(userId, responseStream);
            }
        }

        private void AddSubscriber(int userId, IServerStreamWriter<WeddingUpdateEvent> stream)
        {
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(userId))
                {
                    _subscribers[userId] = new List<IServerStreamWriter<WeddingUpdateEvent>>();
                }

                _subscribers[userId].Add(stream);
                _logger.LogInformation($"Added subscriber to user {userId}. Total subscribers: {_subscribers[userId].Count}");
                Console.WriteLine($"WeddingUpdateGrpcService: Added subscriber to user {userId}. Total subscribers: {_subscribers[userId].Count}");
            }
        }

        private void RemoveSubscriber(int userId, IServerStreamWriter<WeddingUpdateEvent> stream)
        {
            lock (_lock)
            {
                if (_subscribers.ContainsKey(userId))
                {
                    _subscribers[userId].Remove(stream);
                    _logger.LogInformation($"Removed subscriber from user {userId}. Remaining subscribers: {_subscribers[userId].Count}");
                    Console.WriteLine($"WeddingUpdateGrpcService: Removed subscriber from user {userId}. Remaining subscribers: {_subscribers[userId].Count}");

                    if (_subscribers[userId].Count == 0)
                    {
                        _subscribers.Remove(userId);
                    }
                }
            }
        }

        // Static method to notify all subscribers about wedding changes
        public static async Task NotifyWeddingChange(int userId, WeddingUpdateEvent.Types.UpdateType type, WeddingDetails wedding)
        {
            Console.WriteLine($"WeddingUpdateGrpcService: NotifyWeddingChange called for user {userId}, wedding {wedding.WeddingId}, type {type}");

            var update = new WeddingUpdateEvent
            {
                Type = type,
                Wedding = wedding
            };

            List<IServerStreamWriter<WeddingUpdateEvent>> subscribers;
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(userId))
                {
                    Console.WriteLine($"WeddingUpdateGrpcService: No subscribers found for user {userId}");
                    return;
                }

                subscribers = new List<IServerStreamWriter<WeddingUpdateEvent>>(_subscribers[userId]);
                Console.WriteLine($"WeddingUpdateGrpcService: Found {subscribers.Count} subscribers for user {userId}");
            }

            var failedStreams = new List<IServerStreamWriter<WeddingUpdateEvent>>();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    await subscriber.WriteAsync(update);
                    Console.WriteLine($"WeddingUpdateGrpcService: Successfully sent update to a subscriber for user {userId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WeddingUpdateGrpcService: Failed to send update for user {userId}: {ex.Message}");
                    failedStreams.Add(subscriber);
                }
            }

            // Remove failed streams
            if (failedStreams.Count > 0)
            {
                lock (_lock)
                {
                    foreach (var failedStream in failedStreams)
                    {
                        if (_subscribers.ContainsKey(userId))
                        {
                            _subscribers[userId].Remove(failedStream);
                            Console.WriteLine($"WeddingUpdateGrpcService: Removed failed subscriber from user {userId}");
                        }
                    }
                }
            }
        }
    }
}