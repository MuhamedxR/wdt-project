using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{
    public enum OrderStatus
    {
        Pending,
        Preparing,
        Delivered,
        Cancelled
    }
    public class Order
    {
        // Properties
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        [Required, MaxLength(60)]
        public string OrderType { get; set; }

        public OrderStatus Status { get; set; }
        [Phone]
        public string CustomerPhone { get; set; }

        [MaxLength(300)]
        public string? DeliveryAddress { get; set; }
        [MaxLength(100)]
        public string? AssignedEmployeeName { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
        [Range(1, 100000)]
        public decimal TotalAmount { get; set; }



        //----------------Relationships----------------
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Feedback>? Feedbacks { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
