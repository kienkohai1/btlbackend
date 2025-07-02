using BTL.Models;
using BTL.Services;
using BTL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace BTL.Controllers
{
    [Authorize] // Yêu cầu đăng nhập cho toàn bộ controller
    public class ShoppingCartController : Controller
    {
        private readonly QLSKContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShoppingCartController(QLSKContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private ShoppingCart GetCart()
        {
            return ShoppingCart.GetCart(new ServiceProviderWrapper(_context, _httpContextAccessor));
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            var cartItems = cart.GetShoppingCartItems();
            var viewModel = new ShoppingCartViewModel
            {
                ShoppingCart = cart,
                ShoppingCartTotal = cart.GetShoppingCartTotal()
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddToShoppingCart(int ticketId)
        {
            var cart = GetCart();
            var ticket = _context.Tickets.Find(ticketId);
            if (ticket == null)
            {
                TempData["CartMessage"] = "Vé không tồn tại.";
                return RedirectToAction("Index", "Events");
            }

            cart.AddToCart(ticket, 1);
            TempData["CartMessage"] = $"Đã thêm {ticket.Name} vào giỏ hàng.";
            return RedirectToAction("Index");
        }
        public IActionResult AddToCart(int ticketId)
        {
            var ticket = _context.Tickets.Include(t => t.Event).FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null)
            {
                TempData["CartMessage"] = "Vé không tồn tại.";
                return RedirectToAction("Index", "Events");
            }
            return View(ticket);
        }

        public IActionResult RemoveFromShoppingCart(int ticketId)
        {
            var cart = GetCart();
            var ticket = _context.Tickets.Find(ticketId);
            if (ticket == null)
            {
                TempData["CartMessage"] = "Vé không tồn tại.";
                return RedirectToAction("Index");
            }

            cart.RemoveFromCart(ticket);
            TempData["CartMessage"] = $"Đã xóa {ticket.Name} khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            var cart = GetCart();
            cart.ClearCart();
            TempData["CartMessage"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction("Index");
        }

        // Helper class
        public class ServiceProviderWrapper : IServiceProvider
        {
            private readonly QLSKContext _context;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public ServiceProviderWrapper(QLSKContext context, IHttpContextAccessor httpContextAccessor)
            {
                _context = context;
                _httpContextAccessor = httpContextAccessor;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(QLSKContext))
                    return _context;
                if (serviceType == typeof(IHttpContextAccessor))
                    return _httpContextAccessor;
                return null;
            }
        }
    }
}