using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Events
{
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default);
    }
}
