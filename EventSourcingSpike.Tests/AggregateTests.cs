using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace EventSourcingSpike.Tests
{
    [TestFixture]
    public class AggregateTests
    {
        [Test]
        public void Given_A_Non_Empty_Stream_The_Events_Are_Replayed_On_The_Aggregate()
        {
            object[] stream = new[] { new TestEvent("Bob") };
            var subject = new TestAggregate(stream);

            Assert.That(subject.Name, Is.EqualTo("Bob"));
        }

        [Test]
        public void Given_An_Empty_Stream_Ensure_The_Corrected_Event_Is_Added_By_The_Aggregate()
        {
            object[] stream = new object[] { };
            var subject = new TestAggregate(stream);

            subject.SetName("Bill");

            Assert.That(subject.Name, Is.EqualTo("Bill"));

            Assert.That(subject.UndispatchedEvents.Count(), Is.EqualTo(1));
            Assert.That(subject.UndispatchedEvents.First(), Is.InstanceOf<TestEvent>());
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

            protected override void Apply(dynamic @event)
            {
                this.When(@event);
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
