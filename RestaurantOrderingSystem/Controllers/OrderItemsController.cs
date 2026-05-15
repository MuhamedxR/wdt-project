using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    public class OrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserId");

        private string? CurrentUserRole =>
            HttpContext.Session.GetString("Role");

        public override void OnActionExecuting(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (CurrentUserId == null)
                context.Result = RedirectToAction("Login", "Account");

            base.OnActionExecuting(context);
        }


        // ─── INDEX ────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            IQueryable<OrderItem> orderItems = _context.OrderItems
                .Include(o => o.MenuItem)
                .Include(o => o.Order);

            if (CurrentUserRole != "Admin")
                orderItems = orderItems.Where(o =>
                    o.Order.UserId == CurrentUserId);

            return View(await orderItems.ToListAsync());
        }


        // ─── DETAILS ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var orderItem = await _context.OrderItems
                .Include(o => o.MenuItem)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.OrderItemId == id);

            if (orderItem == null) return NotFound();

            if (CurrentUserRole != "Admin" &&
                orderItem.Order.UserId != CurrentUserId)
                return RedirectToAction("AccessDenied", "Home");

            return View(orderItem);
        }


        // ─── CREATE GET ───────────────────────────────────────────────────────
        // orderId is passed from Orders/Create after the order is saved
        public async Task<IActionResult> Create(int? orderId)
        {
            if (orderId == null) return NotFound();

            // ✅ Verify the order exists and belongs to current user
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            if (CurrentUserRole != "Admin" && order.UserId != CurrentUserId)
                return RedirectToAction("AccessDenied", "Home");

            // ✅ Pass order to view so we can show current items + total
            ViewBag.Order = order;

            ViewData["MenuItemId"] = new SelectList(
                _context.MenuItems.OrderBy(m => m.Name),
                "MenuItemId",
                "Name");

            // Pre-fill OrderId in the form
            var newItem = new OrderItem { OrderId = orderId.Value };
            return View(newItem);
        }


        // ─── CREATE POST ──────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Quantity,OrderId,MenuItemId")]
            OrderItem orderItem)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId);

            if (order == null) return NotFound();

            if (CurrentUserRole != "Admin" && order.UserId != CurrentUserId)
                return RedirectToAction("AccessDenied", "Home");

            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.MenuItemId == orderItem.MenuItemId);

            if (menuItem == null) return NotFound();

            // ✅ Server-side: set unit price from MenuItem, never from client
            orderItem.UnitPrice = menuItem.Price;

            // ✅ Remove navigation properties from ModelState validation
            ModelState.Remove(nameof(OrderItem.Order));
            ModelState.Remove(nameof(OrderItem.MenuItem));
            ModelState.Remove(nameof(OrderItem.UnitPrice));

            if (orderItem.Quantity <= 0)
                ModelState.AddModelError("Quantity",
                    "Quantity must be greater than zero.");

            if (ModelState.IsValid)
            {
                _context.Add(orderItem);

                // ✅ Recalculate TotalAmount
                order.TotalAmount += orderItem.Quantity * orderItem.UnitPrice;

                await _context.SaveChangesAsync();

                // ✅ Stay on the same page to add more items
                // User can click "Done" when finished
                return RedirectToAction(nameof(Create),
                    new { orderId = orderItem.OrderId });
            }

            // Reload view data on validation failure
            var orderReload = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId);

            ViewBag.Order = orderReload;

            ViewData["MenuItemId"] = new SelectList(
                _context.MenuItems.OrderBy(m => m.Name),
                "MenuItemId",
                "Name",
                orderItem.MenuItemId);

            return View(orderItem);
        }


        // ─── EDIT GET ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var orderItem = await _context.OrderItems
                .Include(o => o.Order)
                .FirstOrDefaultAsync(o => o.OrderItemId == id);

            if (orderItem == null) return NotFound();

            if (CurrentUserRole != "Admin" &&
                orderItem.Order.UserId != CurrentUserId)
                return RedirectToAction("AccessDenied", "Home");

            ViewData["MenuItemId"] = new SelectList(
                _context.MenuItems.OrderBy(m => m.Name),
                "MenuItemId",
                "Name",
                orderItem.MenuItemId);

            return View(orderItem);
        }


        // ─── EDIT POST ────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("OrderItemId,Quantity,OrderId,MenuItemId")]
            OrderItem orderItem)
        {
            if (id != orderItem.OrderItemId) return NotFound();

            // ✅ Remove navigation props from validation
            ModelState.Remove(nameof(OrderItem.Order));
            ModelState.Remove(nameof(OrderItem.MenuItem));
            ModelState.Remove(nameof(OrderItem.UnitPrice));

            if (orderItem.Quantity <= 0)
                ModelState.AddModelError("Quantity",
                    "Quantity must be greater than zero.");

            if (!ModelState.IsValid)
            {
                ViewData["MenuItemId"] = new SelectList(
                    _context.MenuItems.OrderBy(m => m.Name),
                    "MenuItemId", "Name", orderItem.MenuItemId);
                return View(orderItem);
            }

            var existingOrderItem = await _context.OrderItems
                .Include(o => o.Order)
                .FirstOrDefaultAsync(o => o.OrderItemId == id);

            if (existingOrderItem == null) return NotFound();

            if (CurrentUserRole != "Admin" &&
                existingOrderItem.Order.UserId != CurrentUserId)
                return RedirectToAction("AccessDenied", "Home");

            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.MenuItemId == orderItem.MenuItemId);

            if (menuItem == null) return NotFound();

            try
            {
                // ✅ Subtract old contribution, add new contribution
                existingOrderItem.Order.TotalAmount -=
                    existingOrderItem.Quantity * existingOrderItem.UnitPrice;

                existingOrderItem.Quantity = orderItem.Quantity;
                existingOrderItem.MenuItemId = orderItem.MenuItemId;
                existingOrderItem.UnitPrice = menuItem.Price;

                existingOrderItem.Order.TotalAmount +=
                    existingOrderItem.Quantity * existingOrderItem.UnitPrice;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderItemExists(orderItem.OrderItemId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // ─── DELETE GET ───────────────────────────────────────────────────────
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var orderItem = await _context.OrderItems
                .Include(o => o.MenuItem)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.OrderItemId == id);

            if (orderItem == null) return NotFound();

            if (CurrentUserRole != "Admin" &&
                orderItem.Order.UserId != CurrentUserId)
                return RedirectToAction("AccessDenied", "Home");

            return View(orderItem);
        }


        // ─── DELETE POST ──────────────────────────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItems
                .Include(o => o.Order)
                .FirstOrDefaultAsync(o => o.OrderItemId == id);

            if (orderItem == null) return NotFound();

            if (CurrentUserRole != "Admin" &&
                orderItem.Order.UserId != CurrentUserId)
                return RedirectToAction("AccessDenied", "Home");

            orderItem.Order.TotalAmount -=
                orderItem.Quantity * orderItem.UnitPrice;

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool OrderItemExists(int id) =>
            _context.OrderItems.Any(e => e.OrderItemId == id);
    }
}