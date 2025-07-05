using BTL.Models; // ??m b?o ?� using Models
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Using ?? c� th? d�ng ToListAsync
using System.Diagnostics;
using System.Threading.Tasks;

namespace BTL.Controllers
{
    public class HomeController : Controller
    {
        private readonly QLSKContext _context;
        private readonly ILogger<HomeController> _logger;

        // Constructor ?� ???c th�m QLSKContext
        public HomeController(ILogger<HomeController> logger, QLSKContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Action Index gi? s? l?y danh s�ch s? ki?n
        public async Task<IActionResult> Index()
        {
            // L?y t?t c? s? ki?n t? CSDL
            var events = await _context.Events.ToListAsync();
            // G?i danh s�ch n�y ??n View
            return View(events);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}