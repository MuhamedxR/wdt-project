using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= Current User =================

        private int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserId");

        private string? CurrentUserRole =>
            HttpContext.Session.GetString("Role");

        // ================= Login Protection =================

        public override void OnActionExecuting(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (CurrentUserId == null)
            {
                context.Result =
                    RedirectToAction(
                        "Login",
                        "Account");
            }

            base.OnActionExecuting(context);
        }
        // ================= Orders Index =================

        public async Task<IActionResult> Index(
            int? pageNumber,
            string status = null,
            string sort = "newest")
        {
            IQueryable<Order> orders =
                _context.Orders

                .Include(o => o.User)

                .Include(o => o.Feedbacks)

                .Include(o => o.OrderItems);

            // CUSTOMER يرى أوردراته فقط
            if (CurrentUserRole != "Admin")
            {
                orders = orders.Where(o =>
                    o.UserId == CurrentUserId);
            }

            // FILTER BY STATUS
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o =>
                    o.Status.ToString() == status);
            }

            // SORT
            switch (sort)
            {
                case "oldest":

                    orders =
                        orders.OrderBy(o =>
                            o.OrderDate);

                    break;

                case "high":

                    orders =
                        orders.OrderByDescending(o =>
                            o.TotalAmount);

                    break;

                case "low":

                    orders =
                        orders.OrderBy(o =>
                            o.TotalAmount);

                    break;

                default:

                    orders =
                        orders.OrderByDescending(o =>
                            o.OrderDate);

                    break;
            }

            ViewBag.CurrentStatus =
                status;

            ViewBag.CurrentSort =
                sort;

            int pageSize = 6;

            return View(
                await PaginatedList<Order>
                    .CreateAsync(
                        orders.AsNoTracking(),
                        pageNumber ?? 1,
                        pageSize));
        }

        // ================= Order Details =================

        public async Task<IActionResult> Details(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order =
                await _context.Orders

                .Include(o => o.User)

                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)

                .Include(o => o.Feedbacks)

                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Customer يرى أوردراته فقط
            if (CurrentUserRole != "Admin"
                && order.UserId != CurrentUserId)
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            return View(order);
        }

        // ================= Create Order =================

        public IActionResult Create()
        {
            var order = new Order
            {
                OrderDate = DateTime.Now
            };

            return View(order);
        }

        // ================= Create Order POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("OrderDate,OrderType,CustomerPhone,DeliveryAddress,PaymentMethod")]
            Order order)
        {
            // Server-side values
            order.UserId =
                CurrentUserId!.Value;

            order.Status =
                OrderStatus.Pending;

            order.TotalAmount = 0;

            // Remove Auto Fields
            ModelState.Remove(nameof(Order.UserId));
            ModelState.Remove(nameof(Order.User));
            ModelState.Remove(nameof(Order.TotalAmount));
            ModelState.Remove(nameof(Order.Status));
            ModelState.Remove(nameof(Order.OrderItems));
            ModelState.Remove(nameof(Order.Feedbacks));
            ModelState.Remove(nameof(Order.AssignedEmployeeName));

            if (!ModelState.IsValid)
            {
                return View(order);
            }

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            // Redirect To Add Order Items
            return RedirectToAction(
                "Create",
                "OrderItems",
                new { orderId = order.OrderId });
        }

        // ================= Manage Order GET =================

        public async Task<IActionResult> Edit(
            int? id)
        {
            // Admin Only
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var order =
                await _context.Orders

                .Include(o => o.User)

                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)

                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ================= Manage Order POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("OrderId,AssignedEmployeeName,Status")]
            Order order)
        {
            // Admin Only
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            if (id != order.OrderId)
            {
                return NotFound();
            }

            // Remove Navigation Validation
            ModelState.Remove(nameof(Order.User));
            ModelState.Remove(nameof(Order.UserId));
            ModelState.Remove(nameof(Order.OrderItems));
            ModelState.Remove(nameof(Order.Feedbacks));
            ModelState.Remove(nameof(Order.TotalAmount));
            ModelState.Remove(nameof(Order.OrderDate));
            ModelState.Remove(nameof(Order.OrderType));
            ModelState.Remove(nameof(Order.CustomerPhone));
            ModelState.Remove(nameof(Order.PaymentMethod));
            ModelState.Remove(nameof(Order.DeliveryAddress));

            if (!ModelState.IsValid)
            {
                return View(order);
            }

            // Existing Record
            var existingOrder =
                await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            // Admin Updates Only
            existingOrder.Status =
                order.Status;

            existingOrder.AssignedEmployeeName =
                order.AssignedEmployeeName;

            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.OrderId))
                {
                    return NotFound();
                }

                throw;
            }

            TempData["Success"] =
                "Order Updated Successfully";

            return RedirectToAction(nameof(Index));
        }

        // ================= Delete GET =================

        public async Task<IActionResult> Delete(
            int? id)
        {
            // Admin Only
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var order =
                await _context.Orders

                .Include(o => o.User)

                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ================= Delete POST =================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            // Admin Only
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            var order =
                await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= Cancel Order =================

        public async Task<IActionResult> Cancel(
            int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order =
                await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Customer يرى أوردراته فقط
            if (CurrentUserRole != "Admin"
                && order.UserId != CurrentUserId)
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            // Allow Cancel Only If Pending
            if (order.Status != OrderStatus.Pending)
            {
                return RedirectToAction(
                    nameof(Index));
            }

            order.Status =
                OrderStatus.Cancelled;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Order Cancelled Successfully";

            return RedirectToAction(
                nameof(Index));
        }

        // ================= Helper =================

        private bool OrderExists(int id)
        {
            return _context.Orders
                .Any(e => e.OrderId == id);
        }
    }
}