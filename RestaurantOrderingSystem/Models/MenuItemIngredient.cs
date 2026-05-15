using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{

    public class MenuItemIngredient
    {
        //---------------- Properties ----------------
        public int MenuItemId { get; set; }
        public int IngredientId { get; set; }

        [Range(1, 10000)]
        public decimal QuantityRequired { get; set; }

        //----------------Relationships----------------
        public MenuItem MenuItem { get; set; } = null!;
        public Ingredient Ingredient { get; set; } = null!;

     
    }
}
