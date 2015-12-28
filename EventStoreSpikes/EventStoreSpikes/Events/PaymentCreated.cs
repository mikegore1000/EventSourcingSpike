using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStoreSpikes.Events
{
    public class PaymentCreated
    {
        public PaymentCreated()
        {
            EventId = Guid.NewGuid();
        }

        public Guid EventId { get; set; }

        public Guid PaymentId { get; set; }
    }
}
