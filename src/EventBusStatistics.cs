using System;

namespace Sufficit.Events
{
    public class EventBusStatistics
    {
        /// <summary>
        /// Total events published since process start (monotonic counter snapshot).
        /// </summary>
        public long TotalEventsPublished { get; set; }

        /// <summary>
        /// Total events processed by the bus since process start.
        /// </summary>
        public long TotalEventsProcessed { get; set; }

        /// <summary>
        /// Number of failed handler invocations since process start.
        /// </summary>
        public long FailedEvents { get; set; }

        /// <summary>
        /// Number of currently active subscriptions/registered handlers (best-effort).
        /// </summary>
        public int ActiveSubscriptions { get; set; }

        /// <summary>
        /// UTC timestamp of the last event processed or published snapshot time.
        /// </summary>
        public DateTime LastEventTime { get; set; }
    }
}
