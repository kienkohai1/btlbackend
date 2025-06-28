using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BTL.Models;
using Microsoft.AspNetCore.Hosting; // Đã thêm: Cần thiết để truy cập wwwroot
using System.IO; // Đã thêm: Cần thiết để thao tác với file hệ thống

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

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Đã cập nhật: Thêm "ImagePath" và "ImageFile" vào thuộc tính Bind
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DateTime,Description,Location,ImagePath,ImageFile")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Đã thêm: Lấy thông tin Event hiện tại từ DB để giữ lại ImagePath cũ nếu không có file mới
                    // Sử dụng AsNoTracking() để tránh xung đột tracking khi cập nhật
                    var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

                    if (@event.ImageFile != null) // Nếu có file mới được tải lên
                    {
                        // Xóa hình ảnh cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(existingEvent.ImagePath))
                        {
                            // Tạo đường dẫn đầy đủ đến file cũ trong wwwroot
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingEvent.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Tải lên hình ảnh mới (giống như trong Create)
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
                        @event.ImagePath = "/images/" + uniqueFileName; // Cập nhật đường dẫn hình ảnh mới
                    }
                    else // Nếu không có file mới được tải lên, giữ lại ImagePath cũ từ existingEvent
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
            return View(@event);
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