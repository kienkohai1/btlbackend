namespace BTL.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int TicketId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Giá tại thời điểm mua

        public virtual Ticket Ticket { get; set; }
        public virtual Order Order { get; set; }
    }
}