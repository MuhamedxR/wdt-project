using Microsoft.AspNetCore.Mvc;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }



        // ================= Login Page =================

        public IActionResult Login()
        {
            return View();
        }

        // Handle Login Form Submission
        [HttpPost]
        public IActionResult Login(string email, string password)
        {

            // Search for user in database
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);



            // Check if user exists
            if (user == null)
            {
                ViewBag.Error = "Invalid Email Or Password";
                return View();
            }

            // Save UserId in Session
            HttpContext.Session.SetInt32("UserId", user.UserId);

            // Save User Role in Session
            HttpContext.Session.SetString("Role", user.UserRole.ToString());


            // Save User Name in Session
            HttpContext.Session.SetString("Name", user.Name);

            // Redirect to Home Page
            return RedirectToAction("Index", "Home");
        }
        // ================= Login Page =================
        public IActionResult Logout()
        {
            // Remove All Session Data
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }
    }
}
