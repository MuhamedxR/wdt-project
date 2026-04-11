using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantOrderingSystem.Models
{

    public class MenuItemIngredient
    {
        
        public int MenuItemId { get; set; }
        public int IngredientId { get; set; }

   
        public MenuItem MenuItem { get; set; } = null!;
        public Ingredient Ingredient { get; set; } = null!;

     
        public decimal QuantityRequired { get; set; }
    }
}
