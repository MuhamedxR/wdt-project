using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantOrderingSystem.Models
{

    public enum Role
    {
        Customer = 0,
        Admin = 1
    }
   
    public class User
    {

        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public Role UserRole { get; set; }


        public ICollection<Order> Orders { get; set; }
    }
}
