using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BTL.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BTL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging; // Thêm namespace cho logging

namespace BTL.Controllers
{
    public class EventsController : Controller
    {
        private readonly QLSKContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EventsController> _logger; // Thêm logger

        public EventsController(QLSKContext context, IWebHostEnvironment webHostEnvironment, ILogger<EventsController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(string searchQuery)
        {
            if (_context == null || _context.Events == null)
            {
                return View(new List<Event>());
            }

            var events = _context.Events.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                events = events.Where(e => e.Name.ToLower().Contains(searchQuery));
            }

            ViewBag.SearchQuery = searchQuery;
            return View(await events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anEvent = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);

            if (anEvent == null)
            {
                return NotFound();
            }

            ViewData["Tickets"] = await _context.Tickets
                                                .Where(t => t.EventId == id)
                                                .ToListAsync();

            return View(anEvent);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,DateTime,Description,Location,ImageFile")] Event @event)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "Thông tin không hợp lệ: " + string.Join("; ", errors);
                return View(@event);
            }

            try
            {
                if (@event.ImageFile != null)
                {
                    // Kiểm tra tên tệp
                    if (string.IsNullOrWhiteSpace(@event.ImageFile.FileName))
                    {
                        TempData["ErrorMessage"] = "Tên tệp hình ảnh không hợp lệ.";
                        return View(@event);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(@event.ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "Chỉ hỗ trợ tệp hình ảnh (.jpg, .jpeg, .png, .gif).";
                        return View(@event);
                    }

                    if (@event.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Tệp hình ảnh không được vượt quá 5MB.";
                        return View(@event);
                    }

                    // Tạo tên tệp an toàn
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(@event.ImageFile.FileName);
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    try
                    {
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                            _logger.LogInformation("Tạo thư mục: {Folder}", uploadsFolder);
                        }

                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await @event.ImageFile.CopyToAsync(fileStream);
                        }
                        @event.ImagePath = "/images/" + uniqueFileName;
                        _logger.LogInformation("Đã lưu hình ảnh tại: {FilePath}", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi lưu hình ảnh: FileName={FileName}, Path={Path}", @event.ImageFile.FileName, filePath);
                        TempData["ErrorMessage"] = "Lỗi khi lưu hình ảnh: " + ex.Message;
                        return View(@event);
                    }
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo sự kiện thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sự kiện: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Lỗi khi tạo sự kiện: {ex.Message}";
                return View(@event);
            }
        }

        [Authorize(Roles = "Admin")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
                        // Kiểm tra tên tệp
                        if (string.IsNullOrWhiteSpace(@event.ImageFile.FileName))
                        {
                            TempData["ErrorMessage"] = "Tên tệp hình ảnh không hợp lệ.";
                            return View(new EventEditViewModel { Event = @event, Tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync(), NewTicket = new Ticket { EventId = id } });
                        }

                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(@event.ImageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            TempData["ErrorMessage"] = "Chỉ hỗ trợ tệp hình ảnh (.jpg, .jpeg, .png, .gif).";
                            return View(new EventEditViewModel { Event = @event, Tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync(), NewTicket = new Ticket { EventId = id } });
                        }

                        if (@event.ImageFile.Length > 5 * 1024 * 1024)
                        {
                            TempData["ErrorMessage"] = "Tệp hình ảnh không được vượt quá 5MB.";
                            return View(new EventEditViewModel { Event = @event, Tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync(), NewTicket = new Ticket { EventId = id } });
                        }

                        // Xóa tệp cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(existingEvent.ImagePath))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingEvent.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(oldFilePath);
                                    _logger.LogInformation("Đã xóa hình ảnh cũ: {FilePath}", oldFilePath);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Không thể xóa hình ảnh cũ: {FilePath}", oldFilePath);
                                }
                            }
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(@event.ImageFile.FileName);
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        try
                        {
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                                _logger.LogInformation("Tạo thư mục: {Folder}", uploadsFolder);
                            }

                            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                            {
                                await @event.ImageFile.CopyToAsync(fileStream);
                            }
                            @event.ImagePath = "/images/" + uniqueFileName;
                            _logger.LogInformation("Đã lưu hình ảnh tại: {FilePath}", filePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi lưu hình ảnh: FileName={FileName}, Path={Path}", @event.ImageFile.FileName, filePath);
                            TempData["ErrorMessage"] = "Lỗi khi lưu hình ảnh: " + ex.Message;
                            return View(new EventEditViewModel { Event = @event, Tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync(), NewTicket = new Ticket { EventId = id } });
                        }
                    }
                    else
                    {
                        @event.ImagePath = existingEvent.ImagePath;
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sự kiện thành công.";
                    return RedirectToAction(nameof(Index));
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi chỉnh sửa sự kiện: {Message}", ex.Message);
                    TempData["ErrorMessage"] = $"Lỗi khi chỉnh sửa sự kiện: {ex.Message}";
                    return View(new EventEditViewModel { Event = @event, Tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync(), NewTicket = new Ticket { EventId = id } });
                }
            }

            var tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync();
            var viewModel = new EventEditViewModel
            {
                Event = @event,
                Tickets = tickets,
                NewTicket = new Ticket { EventId = id }
            };
            TempData["ErrorMessage"] = "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
                    TempData["TicketCreationMessage"] = "Thêm vé thành công.";
                }
                else
                {
                    TempData["TicketCreationError"] = "Sự kiện không tồn tại.";
                }
            }
            else
            {
                TempData["TicketCreationError"] = "Thông tin vé không hợp lệ. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction("Edit", new { id = newTicket.EventId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditTicket(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
            {
                return NotFound();
            }

            var viewModel = new EventEditViewModel
            {
                Event = ticket.Event,
                Tickets = await _context.Tickets.Where(t => t.EventId == ticket.EventId).ToListAsync(),
                NewTicket = new Ticket { EventId = ticket.EventId }
            };

            ViewData["EditingTicket"] = ticket;
            return View("Edit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditTicket(int ticketId, int eventId, string Name, decimal Price, int QuantityAvailable)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                TempData["TicketCreationError"] = "Vé không tồn tại.";
                return RedirectToAction("Edit", new { id = eventId });
            }

            if (!string.IsNullOrEmpty(Name) && Price >= 0 && QuantityAvailable >= 0)
            {
                ticket.Name = Name;
                ticket.Price = Price;
                ticket.QuantityAvailable = QuantityAvailable;

                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                    TempData["TicketCreationMessage"] = "Cập nhật vé thành công.";
                }
                catch (DbUpdateException)
                {
                    TempData["TicketCreationError"] = "Lỗi khi cập nhật vé. Vui lòng thử lại.";
                }
            }
            else
            {
                TempData["TicketCreationError"] = "Thông tin vé không hợp lệ. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction("Edit", new { id = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTicket(int ticketId, int eventId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                TempData["TicketCreationError"] = "Vé không tồn tại.";
            }
            else
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
                TempData["TicketCreationMessage"] = "Xóa vé thành công.";
            }

            return RedirectToAction("Edit", new { id = eventId });
        }

        [Authorize(Roles = "Admin")]
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
                        try
                        {
                            System.IO.File.Delete(filePath);
                            _logger.LogInformation("Đã xóa hình ảnh: {FilePath}", filePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Không thể xóa hình ảnh: {FilePath}", filePath);
                        }
                    }
                }

                _context.Tickets.RemoveRange(_context.Tickets.Where(t => t.EventId == id));
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa sự kiện thành công.";
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}