using System.Threading;
using System.Threading.Tasks;

namespace Sufficit.Events
{
    public interface IEventHandler<TEvent>
    {
        Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
    }
}
