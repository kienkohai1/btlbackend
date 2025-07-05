using BTL.Models;
using System.ComponentModel.DataAnnotations;

namespace BTL.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email không hợp lệ.")]
        public string CustomerEmail { get; set; }

        public decimal OrderTotal { get; set; }

        public List<ShoppingCartItem>? CartItems { get; set; } // Allow null
    }
}