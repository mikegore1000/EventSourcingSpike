using System;
using System.IO;
using System.Net;
using System.Text;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStoreSpikes.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventStoreSpikes
{
    public class Bob
    {
        public string MyProperty { get; set; }
    }

    // TODO: See what the throughput would be of logging these...
    public class WarningCreated
    {
        public WarningCreated(string message, string category)
        {
            CreatedAt = DateTimeOffset.Now;
            Message = message;
            Category = category;
        }

        public DateTimeOffset CreatedAt { get; set; }

        public string Message { get; set; }

        public string Category { get; set; }
    }

    class Program
    {
        static void Main()
        {
            RunCatchupSubscription();
            // CreateStream();

            Console.WriteLine("Press a key to stop");
            Console.ReadKey();
        }

        private static async void CreateStream()
        {
            var paymentId = Guid.NewGuid();
            var streamId = "Payment_" + paymentId;

            var created = new PaymentCreated { PaymentId = paymentId };
            var accepted = new PaymentAccepted { PaymentId = paymentId };

            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            await connection.ConnectAsync();

            var events = new [] { CreateEventData(created), CreateEventData(accepted) };

            await connection.AppendToStreamAsync(streamId, ExpectedVersion.EmptyStream, events);

            Console.WriteLine("Created stream {0}", streamId);
        }

        private static EventData CreateEventData(dynamic @event)
        {
            var s = new JsonSerializer();

            using (var ms = new MemoryStream())
            {
                using (var w = new StreamWriter(ms, Encoding.UTF8, 1024 * 8, true))
                {
                    s.Serialize(w, @event);
                }

                ms.Position = 0;

                return new EventData((Guid)@event.EventId, @event.GetType().Name, true, ms.ToArray(), null);
            }
        }

        private static async void RunCatchupSubscription()
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            await connection.ConnectAsync();

            // var catchupSubscription = connection.SubscribeToStreamFrom("$stats-127.0.0.1:2113", null, true, RecievedEvent, userCredentials: new UserCredentials("admin", "changeit"));
            var catchupSubscription = connection.SubscribeToAllFrom(null, false, RecievedEvent, userCredentials: new UserCredentials("admin", "changeit"));
        }

        // Picks up events as they are written to the stream (conceptually the same as reading from a message queue as the events are written)
        private static async void RunCompetingConsumers()
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            await connection.ConnectAsync();

            await connection.SubscribeToStreamAsync("$stats-127.0.0.1:2113", false, RecievedEvent, null, new UserCredentials("admin", "changeit"));
        }

        private static void RecievedEvent(EventStoreSubscription s, ResolvedEvent e)
        {
            Console.WriteLine(e.Event);
        }

        private static void RecievedEvent(EventStoreCatchUpSubscription s, ResolvedEvent e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Event Id: {0} created {1}", e.OriginalEventNumber, e.OriginalEvent.Created);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Encoding.UTF8.GetString(e.OriginalEvent.Data));
        }
    }
}
