namespace RestaurantOrderingSystem.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }


        public string OrderType { get; set; }

        public string CustomerPhone { get; set; }

        public string DeliveryAddress { get; set; }

        public string AssignedEmployeeName { get; set; }

        public string PaymentMethod { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
        public Feedback Feedback { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
