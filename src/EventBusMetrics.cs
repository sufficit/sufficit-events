using System;
using System.Diagnostics.Metrics;
using System.Threading;

namespace Sufficit.Events
{
    /// <summary>
    /// Internal metrics holder that also emits System.Diagnostics.Metrics counters.
    /// Keeps thread-safe long counters for snapshotting while emitting to a Meter for external collectors.
    /// </summary>
    public class EventBusMetrics
    {
        private long _published;
        private long _processed;
        private long _errors;

        // Static meter and counters so they are shared and reusable by collectors.
        private static readonly Meter s_meter = new Meter("Sufficit.Events.EventBus", "1.0");
        private static readonly Counter<long> s_publishedCounter = s_meter.CreateCounter<long>("sufficit.events.published", description: "Total events published");
        private static readonly Counter<long> s_processedCounter = s_meter.CreateCounter<long>("sufficit.events.processed", description: "Total events processed");
        private static readonly Counter<long> s_errorCounter = s_meter.CreateCounter<long>("sufficit.events.errors", description: "Total event processing errors");

        public long Published => Interlocked.Read(ref _published);
        public long Processed => Interlocked.Read(ref _processed);
        public long Errors => Interlocked.Read(ref _errors);

        internal void IncrementPublished()
        {
            Interlocked.Increment(ref _published);
            s_publishedCounter.Add(1);
        }

        internal void IncrementProcessed()
        {
            Interlocked.Increment(ref _processed);
            s_processedCounter.Add(1);
        }

        internal void IncrementErrors()
        {
            Interlocked.Increment(ref _errors);
            s_errorCounter.Add(1);
        }
    }
}
