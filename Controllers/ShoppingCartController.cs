using BTL.Models;
using BTL.Services;
using BTL.ViewModels;
using Microsoft.AspNetCore.Mvc;
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

        public ViewResult Index()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            var scvm = new ShoppingCartViewModel
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal()
            };
            return View(scvm);
        }

        public RedirectToActionResult AddToShoppingCart(int ticketId)
        {
            var selectedTicket = _context.Tickets.FirstOrDefault(p => p.Id == ticketId);
            if (selectedTicket != null)
            {
                _shoppingCart.AddToCart(selectedTicket, 1);
            }
            return RedirectToAction("Index");
        }
    }
}