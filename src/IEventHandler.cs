using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Events
{
    /// <summary>
    /// Contract for an event handler responsible for processing events of type <typeparamref name="TEvent"/>.
    /// Implementations are resolved by dependency injection and invoked by <see cref="EventBus"/>.
    /// </summary>
    /// <typeparam name="TEvent">The event payload type that this handler processes.</typeparam>
    public interface IEventHandler<TEvent>
    {
        /// <summary>
        /// Handle an event asynchronously.
        /// </summary>
        /// <param name="eventData">The event instance to process. Never modify the object unless documented by the event type.</param>
        /// <param name="cancellationToken">Cancellation token that should be observed to cancel processing.</param>
        /// <returns>A task that completes when handling is finished. Exceptions should be propagated to the caller.</returns>
        Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
    }
}
