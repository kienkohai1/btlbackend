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
        private readonly IWebHostEnvironment _webHostEnvironment; // Đã thêm: Khai báo biến này

        // Constructor đã được cập nhật để tiêm IWebHostEnvironment
        public EventsController(QLSKContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment; // Đã thêm: Gán giá trị
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Đã cập nhật: Thêm "ImageFile" vào thuộc tính Bind
        public async Task<IActionResult> Create([Bind("Id,Name,DateTime,Description,Location,ImageFile")] Event @event)
        {
            // ModelState.IsValid kiểm tra cả các Data Annotations trong Event model của bạn
            if (ModelState.IsValid)
            {
                // Đã thêm: Logic xử lý tải lên hình ảnh
                if (@event.ImageFile != null) // Kiểm tra nếu có file được tải lên
                {
                    // Tạo một tên file duy nhất để tránh trùng lặp
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + @event.ImageFile.FileName;
                    // Xác định đường dẫn thư mục images trong wwwroot
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                    // Tạo thư mục nếu nó không tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Kết hợp đường dẫn đầy đủ đến file
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file vào thư mục wwwroot/images
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await @event.ImageFile.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn tương đối của hình ảnh vào cơ sở dữ liệu
                    @event.ImagePath = "/images/" + uniqueFileName;
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        // Action này được cập nhật để sử dụng EventEditViewModel.
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

            // Tải danh sách các vé thuộc về sự kiện này.
            var tickets = await _context.Tickets
                                        .Where(t => t.EventId == id)
                                        .ToListAsync();

            // Tạo ViewModel, đóng gói tất cả dữ liệu cần thiết.
            var viewModel = new EventEditViewModel
            {
                Event = anEvent,
                Tickets = tickets,
                // Khởi tạo NewTicket và gán sẵn EventId cho form thêm nhanh.
                NewTicket = new Ticket { EventId = anEvent.Id }
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        // Action này giữ nguyên logic xử lý file và cập nhật sự kiện của bạn.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DateTime,Description,Location,ImagePath,ImageFile")] Event @event)
        {
            // Giữ nguyên toàn bộ mã nguồn xử lý cập nhật sự kiện của bạn ở đây...
            // Nó đã được viết rất tốt.
            if (id != @event.Id)
            {
                return NotFound();
            }

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

            // Nếu không hợp lệ, tải lại dữ liệu và hiển thị lại trang Edit.
            var tickets = await _context.Tickets.Where(t => t.EventId == id).ToListAsync();
            var viewModel = new EventEditViewModel
            {
                Event = @event,
                Tickets = tickets,
                NewTicket = new Ticket { EventId = id }
            };
            return View(viewModel);
        }


        // === ACTION MỚI ĐỂ XỬ LÝ VIỆC TẠO VÉ NHANH ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicket(EventEditViewModel viewModel)
        {
            // Chúng ta chỉ quan tâm đến dữ liệu của vé mới từ form.
            var newTicket = viewModel.NewTicket;

            // Kiểm tra dữ liệu vé mới có hợp lệ không.
            if (!string.IsNullOrEmpty(newTicket.Name) && newTicket.Price >= 0 && newTicket.QuantityAvailable >= 0)
            {
                // Kiểm tra xem sự kiện có thực sự tồn tại không.
                var anEvent = await _context.Events.FindAsync(newTicket.EventId);
                if (anEvent != null)
                {
                    _context.Tickets.Add(newTicket);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Nếu dữ liệu không hợp lệ, có thể gửi một thông báo lỗi tạm thời.
                TempData["TicketCreationError"] = "Thông tin vé không hợp lệ. Vui lòng thử lại.";
            }

            // Sau khi xử lý xong, chuyển hướng người dùng quay lại chính trang Edit.
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
                // Đã thêm: Xóa hình ảnh vật lý nếu tồn tại
                if (!string.IsNullOrEmpty(@event.ImagePath))
                {
                    // Tạo đường dẫn đầy đủ đến file trong wwwroot
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