using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Events
{
    /// <summary>
    /// Extension methods for IEventBus that provide resilient publishing capabilities
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// Publishes an event/metric using fire-and-forget pattern (executes in background, ignores exceptions silently)
        /// Uses the resilient EventBus.PublishAsync method that returns exceptions instead of throwing them
        /// </summary>
        /// <param name="eventBus">The event bus instance</param>
        /// <param name="eventData">The event or metric data to publish</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static Task PublishFireAndForgetAsync<T>(this IEventBus eventBus, T eventData, CancellationToken cancellationToken = default)
        {
            // True fire-and-forget: execute in background thread without blocking caller
            _ = Task.Run(async () =>
            {
                _ = await eventBus.PublishAsync(eventData, cancellationToken);
                // Fire-and-forget: we intentionally ignore any returned exceptions
            }, cancellationToken);

            // Return completed task immediately - caller doesn't wait
            return Task.CompletedTask;
        }
    }
}
