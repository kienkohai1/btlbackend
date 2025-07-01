using BTL.Models;
using BTL.Services;
using BTL.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BTL.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ShoppingCart _shoppingCart;
        private readonly QLSKContext _context;

        public ShoppingCartController(ShoppingCart shoppingCart, QLSKContext context)
        {
            _shoppingCart = shoppingCart;
            _context = context;
        }

        // GET: /ShoppingCart/
        // Hiển thị trang giỏ hàng.
        public IActionResult Index()
        {
            // Lấy các sản phẩm trong giỏ hàng. Dòng code thừa đã được loại bỏ.
            var items = _shoppingCart.GetShoppingCartItems();

            var viewModel = new ShoppingCartViewModel
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal()
            };

            return View(viewModel);
        }

        // Fix for CS0029: The method AddToCart in ShoppingCart returns void, not bool.  
        // Update the code to remove the assignment to a boolean variable and handle the logic accordingly.  

        public IActionResult AddToShoppingCart(int ticketId)
        {
            var selectedTicket = _context.Tickets.FirstOrDefault(p => p.Id == ticketId);

            if (selectedTicket != null)
            {
                _shoppingCart.AddToCart(selectedTicket, 1);

                // Assume the addition is successful and set a success message.  
                TempData["CartMessage"] = $"Đã thêm vé '{selectedTicket.Name}' vào giỏ hàng!";
            }
            else
            {
                TempData["CartMessage"] = "Lỗi: Không tìm thấy vé bạn chọn.";
            }

            return RedirectToAction("Index");
        }

        // === CẢI THIỆN: BỔ SUNG CÁC ACTION CÒN THIẾU ===

        // GET: /ShoppingCart/RemoveFromShoppingCart/{id}
        // Giảm 1 đơn vị sản phẩm hoặc xóa nếu chỉ còn 1.
        public IActionResult RemoveFromShoppingCart(int ticketId)
        {
            var selectedTicket = _context.Tickets.FirstOrDefault(p => p.Id == ticketId);

            if (selectedTicket != null)
            {
                _shoppingCart.RemoveFromCart(selectedTicket);
            }

            return RedirectToAction("Index");
        }

        // GET: /ShoppingCart/ClearCart
        // Xóa toàn bộ giỏ hàng.
        public IActionResult ClearCart()
        {
            _shoppingCart.ClearCart();
            TempData["CartMessage"] = "Giỏ hàng của bạn đã được dọn trống.";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult AddToCart(int ticketId)
        {
            var ticket = _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
            {
                TempData["CartMessage"] = "Lỗi: Không tìm thấy vé bạn chọn.";
                return RedirectToAction("Index", "Events");
            }

            return View(ticket);
        }
    }
}