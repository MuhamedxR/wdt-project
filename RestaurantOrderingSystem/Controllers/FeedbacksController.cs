using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    public class FeedbacksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= Session Helpers =================

        private int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserId");

        private string? CurrentUserRole =>
            HttpContext.Session.GetString("Role");

        // ================= Login Check =================

        public override void OnActionExecuting(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (CurrentUserId == null)
            {
                context.Result =
                    RedirectToAction("Login", "Account");
            }

            base.OnActionExecuting(context);
        }

        // ================= Admin Feedback List =================

        public async Task<IActionResult> Index()
        {
            // Admin Only
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            var feedbacks =
                await _context.Feedbacks
                .Include(f => f.Order)
                .ThenInclude(o => o.User)
                .ToListAsync();

            return View(feedbacks);
        }

        // ================= Feedback Details =================

        public async Task<IActionResult> Details(int? id)
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

            var feedback =
                await _context.Feedbacks
                .Include(f => f.Order)
                .ThenInclude(o => o.User)

                .Include(f => f.Order)
                .ThenInclude(o => o.OrderItems)

                .ThenInclude(oi => oi.MenuItem)

                .FirstOrDefaultAsync(f =>
                    f.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // ================= Create Feedback =================

        public IActionResult Create(int orderId)
        {
            // Get Order
            var order =
                _context.Orders
                .Include(o => o.Feedbacks)
                .FirstOrDefault(o =>
                    o.OrderId == orderId);

            // Check Order Exists
            if (order == null)
            {
                return NotFound();
            }

            // Customer Can Access Only His Orders
            if (CurrentUserRole != "Admin"
                && order.UserId != CurrentUserId)
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            // Feedback Allowed Only For Delivered Orders
            if (order.Status != OrderStatus.Delivered)
            {
                return RedirectToAction(
                    "Index",
                    "Orders");
            }

            // Prevent Duplicate Feedback
            if (order.Feedbacks != null
     && order.Feedbacks.Count >= 2)
            {
                return RedirectToAction(
                    "Index",
                    "Orders");
            }

            // Send OrderId To View
            var feedback = new Feedback
            {
                OrderId = orderId
            };

            return View(feedback);
        }

        // ================= Create Feedback POST =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Comment,Rating,OrderId")]
            Feedback feedback)
        {
            // Get Order
            var order =
                await _context.Orders
                .Include(o => o.Feedbacks)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == feedback.OrderId);

            if (order == null)
            {
                return NotFound();
            }

            // Customer Can Access Only His Orders
            if (CurrentUserRole != "Admin"
                && order.UserId != CurrentUserId)
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            // Allow Feedback Only After Delivery
            if (order.Status != OrderStatus.Delivered)
            {
                return RedirectToAction(
                    "Index",
                    "Orders");
            }

            // Prevent Duplicate Feedback
            if (order.Feedbacks != null
     && order.Feedbacks.Count >= 2)
            {
                return RedirectToAction(
                    "Index",
                    "Orders");
            }

            if (ModelState.IsValid)
            {
                _context.Feedbacks.Add(feedback);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Thank You For Your Feedback ❤️";

                return RedirectToAction(
                    "Index",
                    "Orders");
            }

            return View(feedback);
        }

        // ================= Delete Feedback =================

        public async Task<IActionResult> Delete(int? id)
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

            var feedback =
                await _context.Feedbacks
                .Include(f => f.Order)
                .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(f =>
                    f.FeedbackId == id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // ================= Delete Feedback POST =================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Admin Only
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            var feedback =
                await _context.Feedbacks
                .FindAsync(id);

            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= Helper =================

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks
                .Any(e => e.FeedbackId == id);
        }
    }
}