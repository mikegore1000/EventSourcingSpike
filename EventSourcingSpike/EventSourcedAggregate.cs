using System;
using System.Collections.Generic;

namespace EventSourcingSpike
{
    public abstract class EventSourcedAggregate
    {
        private readonly List<object> undispatchedEvents = new List<object>();

        protected EventSourcedAggregate(IEnumerable<object> events)
        {
            foreach (var e in events)
            {
                Apply(e);
            }
        }

        public IEnumerable<object> UndispatchedEvents
        {
            get { return undispatchedEvents; }
        }

        protected void Append(object @event)
        {
            undispatchedEvents.Add(@event);
            Apply(@event);
        }

        protected abstract void Apply(dynamic @event);
    }
}
