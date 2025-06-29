using Microsoft.AspNetCore.Mvc;
using BTL.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BTL.Controllers
{
    public class TicketsController : Controller
    {
        private readonly QLSKContext _context;

        public TicketsController(QLSKContext context)
        {
            _context = context;
        }

        // GET: Tickets/Create?eventId=5
        // Chuẩn bị form để tạo vé cho một sự kiện cụ thể.
        public async Task<IActionResult> Create(int eventId)
        {
            var anEvent = await _context.Events.FindAsync(eventId);
            if (anEvent == null)
            {
                return NotFound();
            }

            var ticket = new Ticket
            {
                EventId = eventId
            };

            // Truyền tên sự kiện để người dùng biết họ đang thao tác trên sự kiện nào.
            ViewBag.EventName = anEvent.Name;

            return View(ticket);
        }

        // POST: Tickets/Create
        // Xử lý dữ liệu từ form khi người dùng nhấn nút "Tạo vé".
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,QuantityAvailable,EventId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                // === THAY ĐỔI DUY NHẤT LÀ Ở ĐÂY ===
                // Thay vì chuyển hướng về "Details", chúng ta chuyển hướng về "Edit"
                // của EventsController để người dùng có thể tiếp tục quản lý sự kiện.
                return RedirectToAction("Edit", "Events", new { id = ticket.EventId });
            }

            // Nếu model không hợp lệ, tải lại tên sự kiện và hiển thị lại form.
            var anEvent = await _context.Events.FindAsync(ticket.EventId);
            if (anEvent != null)
            {
                ViewBag.EventName = anEvent.Name;
            }

            return View(ticket);
        }
    }
}
