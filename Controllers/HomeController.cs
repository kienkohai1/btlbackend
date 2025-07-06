using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL.Models;

namespace BTL.Controllers
{
    public class HomeController : Controller
    {
        private readonly QLSKContext _context;

        public HomeController(QLSKContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> Index(string searchQuery)
        {
            if (_context == null || _context.Events == null)
            {
                ViewBag.EventsWithTickets = new object[] { };
                ViewBag.SearchQuery = searchQuery;
                return View();
            }

            var events = _context.Events
                .Include(e => e.Tickets)
                .AsQueryable();

            // �p d?ng b? l?c t�m ki?m n?u c� searchQuery
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                events = events.Where(e => e.Name.ToLower().Contains(searchQuery));
            }

            // Chu?n b? danh s�ch s? ki?n v?i t?ng s? v� c�n l?i
            var eventsWithTickets = await events
                .Select(e => new
                {
                    Event = e,
                    TotalTicketsAvailable = e.Tickets.Sum(t => t.QuantityAvailable)
                })
                .ToListAsync();

            ViewBag.EventsWithTickets = eventsWithTickets;
            ViewBag.SearchQuery = searchQuery;

            return View();
        }
    }
}