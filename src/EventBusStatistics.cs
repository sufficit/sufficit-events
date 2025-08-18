using System;

namespace Sufficit.Events
{
    public class EventBusStatistics
    {
        public long TotalEventsPublished { get; set; }
        public long TotalEventsProcessed { get; set; }
        public long FailedEvents { get; set; }
        public int ActiveSubscriptions { get; set; }
        public DateTime LastEventTime { get; set; }
    }
}
