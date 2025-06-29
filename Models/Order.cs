using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BTL.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string CustomerEmail { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public decimal OrderTotal { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}