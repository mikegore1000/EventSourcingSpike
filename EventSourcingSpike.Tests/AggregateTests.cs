using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace EventSourcingSpike.Tests
{
    public class AggregateTests
    {
        [Fact]
        public void Given_A_Non_Empty_Stream_The_Events_Are_Replayed_On_The_Aggregate()
        {
            object[] stream = { new TestEvent("Bob") };
            var subject = new TestAggregate(stream);

            Assert.Equal("Bob", subject.Name);
        }

        [Fact]
        public void Given_An_Empty_Stream_Ensure_The_Corrected_Event_Is_Added_By_The_Aggregate()
        {
            object[] stream = { };
            var subject = new TestAggregate(stream);

            subject.SetName("Bill");

            Assert.Equal("Bill", subject.Name);
            Assert.Equal(1, subject.UndispatchedEvents.Count());
            Assert.IsType<TestEvent>(subject.UndispatchedEvents.First());
        }

        private class TestAggregate : EventSourcedAggregate
        {
            internal TestAggregate(IEnumerable<object> events) : base(events)
            {
            }

            // requires it.
            public string Name { get; private set; }

            // Don't mutate state directly in your public methods, rather check that state should be mutated
            // and apply an event to handle mutating the state of the aggregate.
            public void SetName(string name)
            {
                Append(new TestEvent(name));
            }

            protected override void Apply(object @event)
            {
                this.When((dynamic)@event);
            }

            private void When(TestEvent e)
            {
                Name = e.Name;
            }
        }

        class TestEvent
        {
            internal TestEvent(string name)
            {
                Name = name;
            }

            public string Name { get; set; }
        }
    }
}
