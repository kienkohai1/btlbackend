using BTL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTL.Services
{
    public class ShoppingCart
    {
        private readonly QLSKContext _context;
        public string ShoppingCartId { get; set; }
        public List<ShoppingCartItem> ShoppingCartItems { get; set; }

        private ShoppingCart(QLSKContext context)
        {
            _context = context;
        }

        public static ShoppingCart GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            var context = services.GetService<QLSKContext>();

            string cartId = session.GetString("CartId") ?? Guid.NewGuid().ToString();
            session.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }

        public void AddToCart(Ticket ticket, int quantity)
        {
            var shoppingCartItem = _context.ShoppingCartItems.SingleOrDefault(
                s => s.Ticket.Id == ticket.Id && s.ShoppingCartId == ShoppingCartId);

            if (shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    Ticket = ticket,
                    Quantity = quantity
                };
                _context.ShoppingCartItems.Add(shoppingCartItem);
            }
            else
            {
                shoppingCartItem.Quantity += quantity;
            }
            _context.SaveChanges();
        }

        public int RemoveFromCart(Ticket ticket)
        {
            var shoppingCartItem = _context.ShoppingCartItems.SingleOrDefault(
                s => s.Ticket.Id == ticket.Id && s.ShoppingCartId == ShoppingCartId);

            int localQuantity = 0;
            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Quantity > 1)
                {
                    shoppingCartItem.Quantity--;
                    localQuantity = shoppingCartItem.Quantity;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(shoppingCartItem);
                }
            }
            _context.SaveChanges();
            return localQuantity;
        }

        public List<ShoppingCartItem> GetShoppingCartItems()
        {
            return ShoppingCartItems ??= _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .Include(s => s.Ticket)
                .ToList();
        }

        public void ClearCart()
        {
            var cartItems = _context.ShoppingCartItems.Where(cart => cart.ShoppingCartId == ShoppingCartId);
            _context.ShoppingCartItems.RemoveRange(cartItems);
            _context.SaveChanges();
        }

        public decimal GetShoppingCartTotal()
        {
            var total = _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .Select(c => c.Ticket.Price * c.Quantity).Sum();
            return total;
        }
    }
}