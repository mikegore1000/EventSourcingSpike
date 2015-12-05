using System;
using System.Collections.Generic;

namespace EventSourcingSpike
{
    public class Projection
    {
        private List<object> undispatchedEvents = new List<object>();
        private Dictionary<Type, Action<object>> projectionFuncs = new Dictionary<Type, Action<object>>();

        public IEnumerable<object> UndispatchedEvents
        {
            get { return undispatchedEvents; }
        }

        protected void Project<TEvent>(Action<TEvent> projectionFunc)
        {
            projectionFuncs.Add(typeof(TEvent), e => projectionFunc((TEvent)e));
        }

        protected void Append(object @event)
        {
            undispatchedEvents.Add(@event);
            ApplyEvent(@event);
        }
        
        protected void Apply(IEnumerable<object> events)
        {
            foreach(var @event in events)
            {
                ApplyEvent(@event);
            }
        }

        private void ApplyEvent(object @event)
        {
            Action<object> func;

            if(projectionFuncs.TryGetValue(@event.GetType(), out func))
            {
                func(@event);
            }
        }
    }
}
