using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantOrderingSystem.Models
{

    public class Feedback
    {
        public int FeedbackId { get; set; }

        public string Comment { get; set; }

        public int Rating { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; } // Allow null for orders without feedback
    }
}
