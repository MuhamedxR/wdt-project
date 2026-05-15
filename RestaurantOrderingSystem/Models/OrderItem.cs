using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{
  
    

    public class OrderItem
    {
        
//----------------Properties----------------

        public int OrderItemId { get; set; }

        [Range(1, 200)]
        public int Quantity { get; set; }
        [Range(1, 100000)]
        public decimal UnitPrice { get; set; }


//----------------Relationships----------------

        public int OrderId { get; set; }
        public Order Order { get; set; }


        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }
    }
}
