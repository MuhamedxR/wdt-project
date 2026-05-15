using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;
using RestaurantOrderingSystem.Helpers;
namespace RestaurantOrderingSystem.Controllers
{
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Current Logged User Id
        private int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserId");

        // Current Logged User Role
        private string? CurrentUserRole =>
            HttpContext.Session.GetString("Role");

      // Check Login Before Protected Actions

public override void OnActionExecuting(
    Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
{
    var actionName =
        context.RouteData.Values["action"]?.ToString();

    // PUBLIC ACTIONS

    if (actionName == "Index" ||
        actionName == "Details")
    {
        base.OnActionExecuting(context);

        return;
    }

    // REQUIRE LOGIN

    if (CurrentUserId == null)
    {
        context.Result =
            RedirectToAction(
                "Login",
                "Account");
    }

    base.OnActionExecuting(context);
}

        public async Task<IActionResult> Index(
     string category,
     string search,
     string sort,
     int? pageNumber)
        {
            ViewBag.Categories =
                await _context.MenuItems
                    .Select(m => m.Category)
                    .Distinct()
                    .ToListAsync();

            ViewBag.Search =
                search;

            ViewBag.CurrentCategory =
                category;

            ViewBag.CurrentSearch =
                search;

            ViewBag.CurrentSort =
                sort;

            var menuItems =
                _context.MenuItems.AsQueryable();

            // FILTER BY CATEGORY

            if (!string.IsNullOrEmpty(category))
            {
                menuItems =
                    menuItems.Where(m =>
                        m.Category == category);
            }

            // SEARCH

            if (!string.IsNullOrEmpty(search))
            {
                menuItems =
                    menuItems.Where(m =>
                        m.Name.Contains(search) ||

                        m.Category.Contains(search) ||

                        m.Description.Contains(search));
            }

            // SORT

            switch (sort)
            {
                case "low":

                    menuItems =
                        menuItems.OrderBy(m =>
                            m.Price);

                    break;

                case "high":

                    menuItems =
                        menuItems.OrderByDescending(m =>
                            m.Price);

                    break;

                case "name":

                    menuItems =
                        menuItems.OrderBy(m =>
                            m.Name);

                    break;

                default:

                    menuItems =
                        menuItems.OrderBy(m =>
                            m.MenuItemId);

                    break;
            }

            // CART

            var cart =
                HttpContext.Session
                .GetObjectFromJson<List<CartItem>>("Cart")

                ?? new List<CartItem>();

            ViewBag.Cart =
                cart;

            // PAGINATION

            int pageSize = 6;

            return View(
                await PaginatedList<MenuItem>
                    .CreateAsync(
                        menuItems.AsNoTracking(),
                        pageNumber ?? 1,
                        pageSize));
        }

        // GET: MenuItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItems.Include(m => m.MenuItemIngredients)
                .ThenInclude(mi => mi.Ingredient)
                .FirstOrDefaultAsync(m =>
                    m.MenuItemId == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // GET: MenuItems/Create
        public IActionResult Create()
        {
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            return View();
        }

        // POST: MenuItems/Create
        // POST: MenuItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Price,Description,Category")] MenuItem menuItem)
        {
            if (CurrentUserRole != "Admin")
                return RedirectToAction("AccessDenied", "Home");

            // Clear auto-generated errors, use only custom ones
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(menuItem.Name))
                ModelState.AddModelError("Name", "Name is required.");

            if (string.IsNullOrWhiteSpace(menuItem.Category))
                ModelState.AddModelError("Category", "Category is required.");

            if (string.IsNullOrWhiteSpace(menuItem.Description))
                ModelState.AddModelError("Description", "Description is required.");

            if (menuItem.Price <= 0)
                ModelState.AddModelError("Price", "Price must be greater than zero.");

            bool exists = await _context.MenuItems
                .AnyAsync(m => m.Name.ToLower() == menuItem.Name.ToLower().Trim());

            if (exists)
                ModelState.AddModelError("Name", "This menu item already exists.");

            if (!ModelState.IsValid)
                return View(menuItem);

            menuItem.Name = menuItem.Name.Trim();
            menuItem.Category = menuItem.Category.Trim();
            menuItem.Description = menuItem.Description.Trim();

            _context.Add(menuItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: MenuItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
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

            var menuItem = await _context.MenuItems
                .FindAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // POST: MenuItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MenuItemId,Name,Price,Description,Category")] MenuItem menuItem)
        {
            if (CurrentUserRole != "Admin")
                return RedirectToAction("AccessDenied", "Home");

            // Guard: route id must match form id
            if (id != menuItem.MenuItemId)
                return NotFound();

            // Clear auto-generated ModelState errors before adding custom ones
            ModelState.Clear();

            // --- Custom validations ---
            if (string.IsNullOrWhiteSpace(menuItem.Name))
                ModelState.AddModelError("Name", "Name is required.");

            if (string.IsNullOrWhiteSpace(menuItem.Category))
                ModelState.AddModelError("Category", "Category is required.");

            if (string.IsNullOrWhiteSpace(menuItem.Description))
                ModelState.AddModelError("Description", "Description is required.");

            if (menuItem.Price <= 0)
                ModelState.AddModelError("Price", "Price must be greater than zero.");

            // Duplicate name check — exclude THIS item by its own ID
            bool nameConflict = await _context.MenuItems
                .AnyAsync(m =>
                    m.Name.ToLower() == menuItem.Name.ToLower().Trim() &&
                    m.MenuItemId != menuItem.MenuItemId);

            if (nameConflict)
                ModelState.AddModelError("Name", "Another menu item with this name already exists.");

            if (!ModelState.IsValid)
                return View(menuItem);

            // --- Save ---
            try
            {
                var existing = await _context.MenuItems
                    .FirstOrDefaultAsync(m => m.MenuItemId == id);

                if (existing == null)
                    return NotFound();

                existing.Name = menuItem.Name.Trim();
                existing.Price = menuItem.Price;
                existing.Description = menuItem.Description.Trim();
                existing.Category = menuItem.Category.Trim();

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(menuItem.MenuItemId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: MenuItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
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

            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m =>
                    m.MenuItemId == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // POST: MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Home");
            }

            var menuItem = await _context.MenuItems
                .FindAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            bool usedInOrders =
                await _context.OrderItems
                    .AnyAsync(o =>
                        o.MenuItemId == id);

            if (usedInOrders)
            {
                TempData["Error"] =
                    "Cannot delete menu item because it is used in orders.";

                return RedirectToAction(nameof(Index));
            }

            _context.MenuItems.Remove(menuItem);

            await _context.SaveChangesAsync();          

            return RedirectToAction(nameof(Index));
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e =>
                e.MenuItemId == id);
        }
    }
}