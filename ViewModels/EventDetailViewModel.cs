// In Models/EventDetailViewModel.cs (or ViewModels/)
using System.Collections.Generic;
using BTL.Models;
namespace BTL.ViewModels
{
    public class EventDetailViewModel
    {
        public Event Event { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
    }
}