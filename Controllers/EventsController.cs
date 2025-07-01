using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BTL.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BTL.ViewModels;

namespace BTL.Controllers
{
    public class EventsController : Controller
    {
        private readonly QLSKContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EventsController(QLSKContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        // GET: Events/Details/5
        // SỬA ĐỔI: Hoàn nguyên để không sử dụng EventDetailViewModel đã bị xóa.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin sự kiện từ database
            var anEvent = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);

            if (anEvent == null)
            {
                return NotFound();
            }

            // Dùng ViewData để gửi danh sách vé sang View
            ViewData["Tickets"] = await _context.Tickets
                                                .Where(t => t.EventId == id)
                                                .ToListAsync();

            // Trả về View với model là đối tượng Event
            return View(anEvent);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,DateTime,Description,Location,ImageFile")] Event @event)
        {
            if (ModelState.IsValid)
            {
                if (@event.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + @event.ImageFile.FileName;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await @event.ImageFile.CopyToAsync(fileStream);
                    }

                    @event.ImagePath = "/images/" + uniqueFileName;
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anEvent = await _context.Events.FindAsync(id);
            if (anEvent == null)
            {
                return NotFound();
            }

            var tickets = await _context.Tickets
                                        .Where(t => t.EventId == id)
                                        .ToListAsync();

            var viewModel = new EventEditViewModel
            {
                Event = anEvent,
                Tickets = tickets,
                NewTicket = new Ticket { EventId = anEvent.Id }
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DateTime,Description,Location,ImagePath,ImageFile")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Tickets");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    if (@event.ImageFile != null)
                    {
                        if (!string.IsNullOrEmpty(existingEvent.ImagePath))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingEvent.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + @event.ImageFile.FileName;
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await @event.ImageFile.CopyToAsync(fileStream);
                        }
                        @event.ImagePath = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        @event.ImagePath = existingEvent.ImagePath;
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync();
            var viewModel = new EventEditViewModel
            {
                Event = @event,
                Tickets = tickets,
                NewTicket = new Ticket { EventId = id }
            };
            return View(viewModel);
        }

        // POST: /Events/AddTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicket(EventEditViewModel viewModel)
        {
            var newTicket = viewModel.NewTicket;

            if (!string.IsNullOrEmpty(newTicket.Name) && newTicket.Price >= 0 && newTicket.QuantityAvailable >= 0)
            {
                var anEvent = await _context.Events.FindAsync(newTicket.EventId);
                if (anEvent != null)
                {
                    _context.Tickets.Add(newTicket);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                TempData["TicketCreationError"] = "Thông tin vé không hợp lệ. Vui lòng thử lại.";
            }

            return RedirectToAction("Edit", new { id = newTicket.EventId });
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                if (!string.IsNullOrEmpty(@event.ImagePath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, @event.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}