using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RestaurantOrderingSystem.Models
{

    public enum Role
    {
        Customer = 0,
        Admin = 1
    }

    public class User
    {
        //---------------- Properties ----------------
        public int UserId { get; set; }
        [Required, MaxLength(150)]

        public string Name { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public Role UserRole { get; set; }

//----------------Relationships----------------
        public ICollection<Order> Orders { get; set; }
    }
}
