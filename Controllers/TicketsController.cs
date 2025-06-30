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

        // Các phương thức Create (GET và POST) đã được xóa vì logic tạo vé
        // hiện đã được xử lý bởi phương thức "AddTicket" trong EventsController
        // và form thêm nhanh trên trang Events/Edit.

        // Trong tương lai, bạn có thể thêm các Action mới vào đây để quản lý
        // chi tiết từng loại vé, ví dụ:
        //
        // public async Task<IActionResult> Edit(int? id) { ... }
        //
        // public async Task<IActionResult> Delete(int? id) { ... }
        //
    }
}
