using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Ví dụ: "Vé Thường", "Vé VIP"

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        public int QuantityAvailable { get; set; } // Số lượng vé có sẵn

        // Khóa ngoại đến Event
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }
}