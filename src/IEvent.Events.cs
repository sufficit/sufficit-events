using System;

namespace Sufficit.Events
{
    /// <summary>
    /// Event contract specific to the events library. Inherits from the legacy `Sufficit.IEvent` marker
    /// to remain compatible with projects that use the older marker type.
    /// </summary>
    public interface IEvent : Sufficit.IEvent
    {
    }
}
