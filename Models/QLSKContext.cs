using BTL.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL.Models
{
    public class QLSKContext : DbContext
    {
        public QLSKContext(DbContextOptions<QLSKContext> options) : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}