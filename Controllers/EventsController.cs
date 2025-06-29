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
        // Phương thức này lấy dữ liệu sự kiện và danh sách vé để hiển thị trên form chỉnh sửa.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anEvent = await _context.Events.FindAsync(id);
            if (anEvent == null)
            {
                // Không tìm thấy sự kiện, trả về trang 404 Not Found.
                return NotFound();
            }

            // Tải danh sách các vé thuộc về sự kiện này.
            var tickets = await _context.Tickets
                                        .Where(t => t.EventId == id)
                                        .ToListAsync();

            // Tạo một ViewModel để đóng gói tất cả dữ liệu cần thiết cho View.
            var viewModel = new EventEditViewModel
            {
                Event = anEvent,
                Tickets = tickets
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        // Phương thức này xử lý dữ liệu được gửi từ form chỉnh sửa.
        // Nó bảo mật, xử lý tệp tin và cập nhật thông tin vào cơ sở dữ liệu.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DateTime,Description,Location,ImagePath,ImageFile")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            // ModelState.IsValid kiểm tra xem dữ liệu gửi lên có hợp lệ không
            // (dựa trên các DataAnnotations trong Model 'Event').
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin sự kiện hiện tại từ DB mà không theo dõi nó
                    // để giữ lại ImagePath cũ nếu không có file mới được tải lên.
                    var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra xem người dùng có tải lên file ảnh mới không.
                    if (@event.ImageFile != null)
                    {
                        // Nếu có ảnh cũ, tiến hành xóa nó khỏi server.
                        if (!string.IsNullOrEmpty(existingEvent.ImagePath))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingEvent.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Tải lên hình ảnh mới (logic giống như trong action Create).
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
                        // Cập nhật đường dẫn hình ảnh mới vào đối tượng event.
                        @event.ImagePath = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        // Nếu không có file mới được tải lên, giữ lại đường dẫn ảnh cũ.
                        @event.ImagePath = existingEvent.ImagePath;
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Xử lý trường hợp có xung đột khi cập nhật dữ liệu (ví dụ: người khác đã xóa event này).
                    if (!EventExists(@event.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Nếu thành công, chuyển hướng về trang danh sách sự kiện.
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ (người dùng nhập thiếu/sai dữ liệu):
            // Tải lại danh sách vé để hiển thị lại form một cách đầy đủ.
            var tickets = await _context.Tickets
                                    .Where(t => t.EventId == id)
                                    .ToListAsync();

            // Đóng gói lại ViewModel với dữ liệu người dùng đã nhập và danh sách vé.
            var viewModel = new EventEditViewModel
            {
                Event = @event,
                Tickets = tickets
            };

            // Trả về View với ViewModel để người dùng có thể sửa lại lỗi.
            return View(viewModel);
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