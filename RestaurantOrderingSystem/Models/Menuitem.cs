using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{


    public class MenuItem
    {
        //----------------Properties----------------
        public int MenuItemId { get; set; }
        [Required, MaxLength(150)]

        public string Name { get; set; }
        [Range(1, 10000)]
        public decimal Price { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        public string Category { get; set; }
        //----------------Relationships----------------
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; }

    }
}
