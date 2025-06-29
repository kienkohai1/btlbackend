using System.ComponentModel.DataAnnotations;

namespace BTL.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        public Ticket Ticket { get; set; }
        public int Quantity { get; set; }

        // ID của giỏ hàng (sẽ được quản lý bằng session)
        public string ShoppingCartId { get; set; }
    }
}