using BTL.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL.Models
{
    public class QLSKContext : DbContext
    {
        public QLSKContext(DbContextOptions<QLSKContext> options) : base(options)
        {
        }

        public DbSet<Event> Events { get; set; } // Đại diện cho bảng Events trong cơ sở dữ liệu của bạn
    }
}