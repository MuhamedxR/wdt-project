using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantOrderingSystem.Models
{
  
 
    public class MenuItem
    {
        public int MenuItemId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }


        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; }

    }
}
