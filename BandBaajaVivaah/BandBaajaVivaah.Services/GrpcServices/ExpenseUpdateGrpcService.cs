using BandBaajaVivaah.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandBaajaVivaah.Services.GrpcServices
{
    public class ExpenseUpdateGrpcService : Grpc.ExpenseUpdateService.ExpenseUpdateServiceBase
    {
        private readonly ILogger<ExpenseUpdateGrpcService> _logger;
        private static readonly Dictionary<int, List<IServerStreamWriter<ExpenseUpdateEvent>>> _subscribers = new();
        private static readonly object _lock = new();

        public ExpenseUpdateGrpcService(ILogger<ExpenseUpdateGrpcService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeToExpenseUpdates(
            ExpenseUpdateSubscriptionRequest request,
            IServerStreamWriter<ExpenseUpdateEvent> responseStream,
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

        private void AddSubscriber(int weddingId, IServerStreamWriter<ExpenseUpdateEvent> stream)
        {
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(weddingId))
                {
                    _subscribers[weddingId] = new List<IServerStreamWriter<ExpenseUpdateEvent>>();
                }

                _subscribers[weddingId].Add(stream);
                _logger.LogInformation($"Added subscriber to wedding {weddingId}. Total subscribers: {_subscribers[weddingId].Count}");
            }
        }

        private void RemoveSubscriber(int weddingId, IServerStreamWriter<ExpenseUpdateEvent> stream)
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

        public static async Task NotifyExpenseChange(int weddingId, ExpenseUpdateEvent.Types.UpdateType type, ExpenseDetails expense)
        {
            var update = new ExpenseUpdateEvent
            {
                Type = type,
                Expense = expense
            };

            List<IServerStreamWriter<ExpenseUpdateEvent>> subscribers;
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(weddingId))
                    return;

                subscribers = new List<IServerStreamWriter<ExpenseUpdateEvent>>(_subscribers[weddingId]);
            }

            var failedStreams = new List<IServerStreamWriter<ExpenseUpdateEvent>>();

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
