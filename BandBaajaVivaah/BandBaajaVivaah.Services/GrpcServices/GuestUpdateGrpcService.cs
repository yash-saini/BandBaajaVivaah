using BandBaajaVivaah.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace BandBaajaVivaah.Services.GrpcServices
{
    public class GuestUpdateGrpcService : Grpc.GuestUpdateService.GuestUpdateServiceBase
    {
        private readonly ILogger<GuestUpdateGrpcService> _logger;
        private static readonly Dictionary<int, List<IServerStreamWriter<GuestUpdateEvent>>> _subscribers = new();
        private static readonly object _lock = new();

        public GuestUpdateGrpcService(ILogger<GuestUpdateGrpcService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeToGuestUpdates(
            GuestUpdateSubscriptionRequest request,
            IServerStreamWriter<GuestUpdateEvent> responseStream,
            ServerCallContext context)
        {
            var weddingId = request.WeddingId;

            // Add this client to the subscribers for this wedding
            AddSubscriber(weddingId, responseStream);

            try
            {
                // Keep the stream open until the client disconnects
                await Task.Delay(-1, context.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Client disconnected
            }
            finally
            {
                // Remove the subscriber when they disconnect
                RemoveSubscriber(weddingId, responseStream);
            }
        }

        private void AddSubscriber(int weddingId, IServerStreamWriter<GuestUpdateEvent> stream)
        {
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(weddingId))
                {
                    _subscribers[weddingId] = new List<IServerStreamWriter<GuestUpdateEvent>>();
                }

                _subscribers[weddingId].Add(stream);
                _logger.LogInformation($"Added subscriber to wedding {weddingId}. Total subscribers: {_subscribers[weddingId].Count}");
            }
        }

        private void RemoveSubscriber(int weddingId, IServerStreamWriter<GuestUpdateEvent> stream)
        {
            lock (_lock)
            {
                if (_subscribers.ContainsKey(weddingId))
                {
                    _subscribers[weddingId].Remove(stream);

                    if (_subscribers[weddingId].Count == 0)
                    {
                        _subscribers.Remove(weddingId);
                    }
                }
            }
        }

        // Static method to notify all subscribers about guest changes
        public static async Task NotifyGuestChange(int weddingId, GuestUpdateEvent.Types.UpdateType type, GuestDetails guest)
        {
            var update = new GuestUpdateEvent
            {
                Type = type,
                Guest = guest
            };

            List<IServerStreamWriter<GuestUpdateEvent>> subscribers;
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(weddingId))
                    return;

                subscribers = new List<IServerStreamWriter<GuestUpdateEvent>>(_subscribers[weddingId]);
            }

            var failedStreams = new List<IServerStreamWriter<GuestUpdateEvent>>();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    await subscriber.WriteAsync(update);
                }
                catch (Exception)
                {
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
                        if (_subscribers.ContainsKey(weddingId))
                        {
                            _subscribers[weddingId].Remove(failedStream);
                        }
                    }
                }
            }
        }
    }
}