using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Helpers;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get Cart From Session
            List<CartItem> cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            return View(cart);
        }
        // Add Menu Item To Cart
        public IActionResult AddToCart(int id)
        {
            // Get Menu Item From Database
            var menuItem = _context.MenuItems
                .FirstOrDefault(m =>
                    m.MenuItemId == id);

            // Check If Menu Item Exists
            if (menuItem == null)
            {
                return NotFound();
            }

            // Get Cart From Session
            List<CartItem> cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            // Check If Item Already Exists In Cart
            var existingItem = cart
                .FirstOrDefault(c =>
                    c.MenuItemId == id);

            // If Exists Increase Quantity
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }

            // Else Add New Item
            else
            {
                cart.Add(new CartItem
                {
                    MenuItemId = menuItem.MenuItemId,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = 1
                });
            }

            // Save Cart Back To Session
            HttpContext.Session
                .SetObjectAsJson("Cart", cart);



            // Redirect Back To Menu
            return RedirectToAction(
                "Index",
                "MenuItems");
        }
        // Increase Quantity
        public IActionResult Increase(int id)
        {
            List<CartItem> cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            var item = cart
                .FirstOrDefault(c =>
                    c.MenuItemId == id);

            if (item != null)
            {
                item.Quantity++;
            }

            HttpContext.Session
                .SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");

            //TempData["Success"] =
            //    "Cart cleared successfully";

            return RedirectToAction(
                "Index",
                "MenuItems");
        }
        // ================= ReOrder =================

        public IActionResult ReOrder(int id)
        {
            // Get Old Order With Items
            var order = _context.Orders.Include(o => o.OrderItems)
.ThenInclude(oi => oi.MenuItem)
                .FirstOrDefault(o =>
                    o.OrderId == id);

            // Check Order Exists
            if (order == null)
            {
                return NotFound();
            }

            // Get Cart From Session
            List<CartItem> cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            // Loop Through Old Order Items
            foreach (var item in order.OrderItems)
            {
                // Check If Item Already Exists
                var existingItem =
                    cart.FirstOrDefault(c =>
                        c.MenuItemId ==
                        item.MenuItemId);

                // Increase Quantity
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                }

                // Add New Item
                else
                {
                    cart.Add(new CartItem
                    {
                        MenuItemId =
                            item.MenuItemId,

                        Name =
                            item.MenuItem.Name,

                        Price =
                            item.UnitPrice,

                        Quantity =
                            item.Quantity
                    });
                }
            }

            // Save Cart Back To Session
            HttpContext.Session
                .SetObjectAsJson("Cart", cart);

            // Success Message
            TempData["Success"] =
                "Order Added To Cart Successfully";

            // Show Go To Cart Button
            TempData["ShowCartButton"] = true;

            return RedirectToAction(
                "Index",
                "Orders");
        }

        

        // Decrease Quantity
        public IActionResult Decrease(int id)
        {
            List<CartItem> cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            var item = cart
                .FirstOrDefault(c =>
                    c.MenuItemId == id);

            if (item != null)
            {
                item.Quantity--;

                // Remove If Quantity Becomes Zero
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
            }

            HttpContext.Session
                .SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }


        // Remove Item Completely
        public IActionResult Remove(int id)
        {
            List<CartItem> cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            var item = cart
                .FirstOrDefault(c =>
                    c.MenuItemId == id);

            if (item != null)
            {
                cart.Remove(item);
            }

            HttpContext.Session
                .SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }
        // GET: Checkout
        public IActionResult Checkout()
        {
            // Get Cart From Session
            List<CartItem> cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            // Prevent Checkout If Cart Empty
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            // Open Checkout Page
            return View();
        }

        // POST: Place Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            // Get Cart From Session
            List<CartItem> cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")
                ?? new List<CartItem>();

            // Prevent Empty Cart Checkout
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            // Remove Properties Not Included In Form
            // Remove Fields Not Sent From Checkout Form
            ModelState.Remove("User");

            ModelState.Remove("OrderItems");

            ModelState.Remove("Feedbacks");

            ModelState.Remove("UserId");

            ModelState.Remove("TotalAmount");

            ModelState.Remove("AssignedEmployeeName");

            ModelState.Remove("Status");

            ModelState.Remove("OrderDate");

            // Set Server-Side Values
            order.OrderDate = DateTime.Now;

            order.Status = OrderStatus.Pending;

            order.UserId =
                HttpContext.Session.GetInt32("UserId")
                ?? 0;

            // Calculate Total Amount
            order.TotalAmount =
                cart.Sum(i => i.Total);
            // Print Validation Errors In Output Window
            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState)
                {
                    foreach (var error in item.Value.Errors)
                    {
                        Console.WriteLine(
                            $"Field: {item.Key} => Error: {error.ErrorMessage}");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                // Save Order First
                _context.Orders.Add(order);

                await _context.SaveChangesAsync();

                // Create OrderItems
                foreach (var item in cart)
                {
                    OrderItem orderItem =
                        new OrderItem
                        {
                            OrderId = order.OrderId,

                            MenuItemId = item.MenuItemId,

                            Quantity = item.Quantity,

                            UnitPrice = item.Price
                        };

                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                // Clear Cart After Successful Order
                HttpContext.Session.Remove("Cart");

                // Redirect To Orders Page
                return RedirectToAction(
                    "Index",
                    "Orders");
            }

            return View("Checkout", order);
        }
    }

}
