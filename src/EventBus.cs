using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sufficit.Events
{
    public class EventBus : IEventBus, IDisposable
    {
        private readonly Channel<(Type EventType, object EventData, CancellationToken CancellationToken)> _eventChannel;
        private readonly ChannelWriter<(Type, object, CancellationToken)> _writer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventBus> _logger;
        private readonly Task _processingTask;
        private readonly CancellationTokenSource _cts;
        private readonly EventBusMetrics _metrics;

        /// <summary>
        /// Creates a new instance of <see cref="EventBus"/>.
        /// </summary>
        /// <param name="serviceProvider">Service provider used to resolve handlers and optional logger.</param>
        public EventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = serviceProvider.GetService<ILogger<EventBus>>() ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EventBus>.Instance;
            _cts = new CancellationTokenSource();
            _metrics = new EventBusMetrics();

            var options = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            };

            _eventChannel = Channel.CreateBounded<(Type, object, CancellationToken)>(options);
            _writer = _eventChannel.Writer;

            _processingTask = Task.Run(ProcessEventsAsync, CancellationToken.None);
            _logger.LogInformation("EventBus initialized (Channels) with capacity {Capacity}", options.Capacity);
        }

        /// <summary>
        /// Publishes an event to the bus. The method enqueues the event for background processing. If the provided
        /// <paramref name="eventData"/> is null the publish is ignored and a warning is logged.
        /// </summary>
        /// <typeparam name="TEvent">Type of the event payload.</typeparam>
        /// <param name="eventData">Event instance to publish.</param>
        /// <param name="cancellationToken">Token used while writing to the internal channel.</param>
        /// <returns>A task that completes when the event is queued for processing.</returns>
        public virtual async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        {
            if (eventData == null)
            {
                _logger.LogWarning("Attempted to publish null event of type {EventType}", typeof(TEvent).Name);
                return;
            }

            _metrics.IncrementPublished();

            try
            {
                await _writer.WriteAsync((typeof(TEvent), eventData, cancellationToken), cancellationToken);
                _logger.LogDebug("Event {EventType} queued", typeof(TEvent).Name);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Event publishing cancelled for {EventType}", typeof(TEvent).Name);
            }
            catch (Exception ex)
            {
                _metrics.IncrementErrors();
                _logger.LogError(ex, "Failed to publish event {EventType}", typeof(TEvent).Name);
                throw;
            }
        }

        /// <summary>
        /// Background loop that reads from the internal channel and dispatches events to handlers.
        /// This method runs on a dedicated background task started by the constructor.
        /// </summary>
        /// <returns>A Task representing the background processing loop.</returns>
        private async Task ProcessEventsAsync()
        {
            _logger.LogInformation("EventBus background processor started");

            try
            {
                var reader = _eventChannel.Reader;
                while (await reader.WaitToReadAsync(_cts.Token))
                {
                    while (reader.TryRead(out var item))
                    {
                        var (eventType, eventData, cancellationToken) = item;

                        if (cancellationToken.IsCancellationRequested)
                            continue;

                        try
                        {
                            await ProcessEventImmediately(eventType, eventData, cancellationToken).ConfigureAwait(false);
                            _metrics.IncrementProcessed();
                        }
                        catch (Exception ex)
                        {
                            _metrics.IncrementErrors();
                            _logger.LogError(ex, "Error processing event {EventType}", eventType.Name);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("EventBus background processor cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EventBus background processor failed");
            }
        }

        /// <summary>
        /// Resolves handler implementations for the provided <paramref name="eventType"/> and invokes
        /// their HandleAsync methods synchronously from the background processing task.
        /// Reflection is used to call the generic handler method since the event type is only known at runtime.
        /// </summary>
        /// <param name="eventType">Runtime type of the event to dispatch.</param>
        /// <param name="eventData">Event payload instance.</param>
        /// <param name="cancellationToken">Cancellation token forwarded to the handler invocation.</param>
        /// <returns>A task that completes when all resolved handlers have finished processing the event.</returns>
        private async Task ProcessEventImmediately(Type eventType, object eventData, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                var handlers = scope.ServiceProvider.GetServices(handlerType);

                var handlerTasks = handlers.Select(async handler =>
                {
                    if (handler == null) return;

                    try
                    {
                        var handleMethod = handlerType.GetMethod("HandleAsync");
                        if (handleMethod == null) return;

                        var result = handleMethod.Invoke(handler, new[] { eventData, cancellationToken });

                        if (result is Task task)
                        {
                            await task.ConfigureAwait(false);
                        }

                        _logger.LogDebug("Handler {HandlerType} processed event {EventType}", handler.GetType().Name, eventType.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Handler {HandlerType} failed to process event {EventType}", handler?.GetType().Name, eventType.Name);
                    }
                });

                var valid = handlerTasks.Where(t => t != null).ToArray();
                await Task.WhenAll(valid).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve handlers for event {EventType}", eventType.Name);
            }
        }

        /// <summary>
        /// Returns a snapshot of the internal metrics exposed by the event bus.
        /// The returned object is a simple DTO suitable for serialization and logging.
        /// </summary>
        /// <returns>An <see cref="EventBusStatistics"/> instance containing the latest counters.</returns>
        public EventBusStatistics GetStatistics()
        {
            return new EventBusStatistics
            {
                TotalEventsPublished = _metrics.Published,
                TotalEventsProcessed = _metrics.Processed,
                FailedEvents = _metrics.Errors,
                ActiveSubscriptions = 0,
                LastEventTime = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Disposes the event bus, signals the background processor to finish and releases resources.
        /// The method attempts a graceful shutdown and logs any issues encountered during stop.
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("EventBus disposing - Metrics: {@Metrics}", _metrics);

            try
            {
                _writer.Complete();
                _cts.Cancel();
                _processingTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "EventBus background processor did not shutdown cleanly");
            }
            finally
            {
                _cts.Dispose();
            }
        }
    }
}
