using System.ComponentModel.DataAnnotations;

namespace BTL.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        public Ticket Ticket { get; set; }
        public int Quantity { get; set; }
        public string ShoppingCartId { get; set; }

    }
}