using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{

    public class Feedback
    {

        //----------------Properties----------------

        public int FeedbackId { get; set; }
        [MaxLength(300)]
        public string Comment { get; set; }
        [Range(1, 10)]
        public int Rating { get; set; }

        //----------------Relationships----------------
        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
