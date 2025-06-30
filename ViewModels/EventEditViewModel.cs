using System.Collections.Generic;
using BTL.Models;

namespace BTL.ViewModels
{
    public class EventEditViewModel
    {
        public Event Event { get; set; }

        public IEnumerable<Ticket> Tickets { get; set; }

        public Ticket NewTicket { get; set; }
    }
}
