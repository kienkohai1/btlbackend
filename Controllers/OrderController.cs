using BTL.Models;
using BTL.Services;
using BTL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BTL.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly QLSKContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderController(QLSKContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private ShoppingCart GetCart()
        {
            return ShoppingCart.GetCart(new ShoppingCartController.ServiceProviderWrapper(_context, _httpContextAccessor));
        }

        // GET: /Order/Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            var cartItems = cart.GetShoppingCartItems();
            if (!cartItems.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                OrderTotal = cart.GetShoppingCartTotal(),
                CustomerEmail = User.Identity.Name
            };
            return View(viewModel);
        }

        // POST: /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();
            var cartItems = cart.GetShoppingCartItems();
            if (!cartItems.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            model.CartItems = cartItems;
            model.OrderTotal = cart.GetShoppingCartTotal();

            if (ModelState.IsValid)
            {
                foreach (var item in cartItems)
                {
                    var ticket = await _context.Tickets.FindAsync(item.Ticket.Id);
                    if (ticket == null || ticket.QuantityAvailable < item.Quantity)
                    {
                        TempData["CartMessage"] = $"Vé {item.Ticket.Name} không đủ số lượng.";
                        return RedirectToAction("Index", "ShoppingCart");
                    }
                }

                var order = new Order
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    OrderDate = DateTime.Now,
                    OrderTotal = cart.GetShoppingCartTotal(),
                    OrderDetails = cartItems.Select(item => new OrderDetail
                    {
                        TicketId = item.Ticket.Id,
                        Quantity = item.Quantity,
                        Price = item.Ticket.Price
                    }).ToList()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    var ticket = await _context.Tickets.FindAsync(item.Ticket.Id);
                    ticket.QuantityAvailable -= item.Quantity;
                }

                cart.ClearCart();

                await _context.SaveChangesAsync();

                TempData["CartMessage"] = "Đặt hàng thành công! Cảm ơn bạn đã mua vé.";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: /Order/OrderHistory
        [HttpGet]
        public async Task<IActionResult> OrderHistory()
        {
            var userEmail = User.Identity.Name;
            var orders = await _context.Orders
                .Where(o => o.CustomerEmail == userEmail)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Ticket)
                .ThenInclude(t => t.Event)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
        // GET: /Order/AllOrders
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Ticket)
                .ThenInclude(t => t.Event)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}