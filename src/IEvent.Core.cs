using System;

namespace Sufficit
{
    /// <summary>
    /// Lightweight marker interface used across legacy projects to represent an event payload.
    /// The more specific library-level interface `Sufficit.Events.IEvent` will inherit from this to
    /// preserve backward compatibility while keeping the dedicated event types in the events library.
    /// </summary>
    public interface IEvent
    {
    }
}
