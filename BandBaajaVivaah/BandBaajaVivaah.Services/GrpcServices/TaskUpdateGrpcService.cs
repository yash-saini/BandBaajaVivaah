using BandBaajaVivaah.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandBaajaVivaah.Services.GrpcServices
{
    public class TaskUpdateGrpcService : Grpc.TaskUpdateService.TaskUpdateServiceBase
    {
        private readonly ILogger<TaskUpdateGrpcService> _logger;
        private static readonly Dictionary<int, List<IServerStreamWriter<TaskUpdateEvent>>> _subscribers = new();
        private static readonly object _lock = new();

        public TaskUpdateGrpcService(ILogger<TaskUpdateGrpcService> logger)
        {
            _logger = logger;
        }

        public override async Task SubscribeToTaskUpdates(
            TaskUpdateSubscriptionRequest request,
            IServerStreamWriter<TaskUpdateEvent> responseStream,
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
                _logger.LogInformation($"Client disconnected from wedding {weddingId}");
            }
            finally
            {
                // Remove the subscriber when they disconnect
                RemoveSubscriber(weddingId, responseStream);
            }
        }

        private void AddSubscriber(int weddingId, IServerStreamWriter<TaskUpdateEvent> stream)
        {
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(weddingId))
                {
                    _subscribers[weddingId] = new List<IServerStreamWriter<TaskUpdateEvent>>();
                }

                _subscribers[weddingId].Add(stream);
                _logger.LogInformation($"Added subscriber to wedding {weddingId}. Total subscribers: {_subscribers[weddingId].Count}");
                Console.WriteLine($"TaskUpdateGrpcService: Added subscriber to wedding {weddingId}. Total subscribers: {_subscribers[weddingId].Count}");
            }
        }

        private void RemoveSubscriber(int weddingId, IServerStreamWriter<TaskUpdateEvent> stream)
        {
            lock (_lock)
            {
                if (_subscribers.ContainsKey(weddingId))
                {
                    _subscribers[weddingId].Remove(stream);
                    _logger.LogInformation($"Removed subscriber from wedding {weddingId}. Remaining subscribers: {_subscribers[weddingId].Count}");
                    Console.WriteLine($"TaskUpdateGrpcService: Removed subscriber from wedding {weddingId}. Remaining subscribers: {_subscribers[weddingId].Count}");

                    if (_subscribers[weddingId].Count == 0)
                    {
                        _subscribers.Remove(weddingId);
                    }
                }
            }
        }

        // Static method to notify all subscribers about task changes
        public static async Task NotifyTaskChange(int weddingId, TaskUpdateEvent.Types.UpdateType type, TaskDetails task)
        {
            Console.WriteLine($"TaskUpdateGrpcService: NotifyTaskChange called for wedding {weddingId}, task {task.TaskId}, type {type}");

            var update = new TaskUpdateEvent
            {
                Type = type,
                Task = task
            };

            List<IServerStreamWriter<TaskUpdateEvent>> subscribers;
            lock (_lock)
            {
                if (!_subscribers.ContainsKey(weddingId))
                {
                    Console.WriteLine($"TaskUpdateGrpcService: No subscribers found for wedding {weddingId}");
                    return;
                }

                subscribers = new List<IServerStreamWriter<TaskUpdateEvent>>(_subscribers[weddingId]);
                Console.WriteLine($"TaskUpdateGrpcService: Found {subscribers.Count} subscribers for wedding {weddingId}");
            }

            var failedStreams = new List<IServerStreamWriter<TaskUpdateEvent>>();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    await subscriber.WriteAsync(update);
                    Console.WriteLine($"TaskUpdateGrpcService: Successfully sent update to a subscriber for wedding {weddingId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TaskUpdateGrpcService: Failed to send update for wedding {weddingId}: {ex.Message}");
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
                            Console.WriteLine($"TaskUpdateGrpcService: Removed failed subscriber from wedding {weddingId}");
                        }
                    }
                }
            }
        }
    }
}