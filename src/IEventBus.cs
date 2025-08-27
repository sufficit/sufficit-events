using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Events
{
    /// <summary>
    /// Abstraction for an in-process event bus capable of publishing events to registered handlers.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publish an event instance to the bus. The call enqueues the event for background processing and returns
        /// after the event has been accepted by the bus. Delivery to handlers occurs asynchronously.
        /// The method does not throw exceptions - instead returns them for caller decision.
        /// </summary>
        /// <typeparam name="TEvent">Type of the event payload.</typeparam>
        /// <param name="eventData">Event payload instance to publish.</param>
        /// <param name="cancellationToken">Cancellation token used while enqueuing the event.</param>
        /// <returns>A task that completes with null on success, or the exception that occurred on failure.</returns>
        Task<Exception?> PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default);
    }
}
