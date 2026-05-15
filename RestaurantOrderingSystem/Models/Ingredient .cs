using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{


    public class Ingredient
    {
        //----------------Properties----------------
        public int IngredientId { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [Range(0, 10000)]
        public decimal QuantityInStock { get; set; }
        [Required]
        public string Unit { get; set; }
        [Range(0, 10000)]
        public decimal UnitPrice { get; set; }
        //----------------Relationships----------------
        public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; } = new List<MenuItemIngredient>();


    }
}
