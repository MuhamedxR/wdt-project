using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    public class MenuItemIngredientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuItemIngredientsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Current Logged User Id
        private int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserId");

        // Current Logged User Role
        private string? CurrentUserRole =>
            HttpContext.Session.GetString("Role");


        // Admin Only Access
        public override void OnActionExecuting(
            Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            // Check Login
            if (CurrentUserId == null)
            {
                context.Result =
                    RedirectToAction("Login", "Account");
            }

            // Only Admin Can Access
            if (CurrentUserRole != "Admin")
            {
                context.Result =
                    RedirectToAction(
                        "AccessDenied",
                        "Home");
            }

            base.OnActionExecuting(context);
        }


        // GET: MenuItemIngredients
        public async Task<IActionResult> Index()
        {
            var menuItemIngredients =
                _context.MenuItemIngredients
                    .Include(m => m.Ingredient)
                    .Include(m => m.MenuItem);

            return View(
                await menuItemIngredients.ToListAsync());
        }


        // GET: MenuItemIngredients/Details/5
        public async Task<IActionResult> Details(
            int? menuItemId,
            int? ingredientId)
        {
            if (menuItemId == null ||
                ingredientId == null)
            {
                return NotFound();
            }

            var menuItemIngredient =
                await _context.MenuItemIngredients
                    .Include(m => m.Ingredient)
                    .Include(m => m.MenuItem)
                    .FirstOrDefaultAsync(m =>
                        m.MenuItemId == menuItemId &&
                        m.IngredientId == ingredientId);

            if (menuItemIngredient == null)
            {
                return NotFound();
            }

            return View(menuItemIngredient);
        }


        // GET: MenuItemIngredients/Create
        public IActionResult Create()
        {
            ViewData["IngredientId"] =
                new SelectList(
                    _context.Ingredients,
                    "IngredientId",
                    "Name");

            ViewData["MenuItemId"] =
                new SelectList(
                    _context.MenuItems,
                    "MenuItemId",
                    "Name");

            return View();
        }


        // POST: MenuItemIngredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "MenuItemId,IngredientId,QuantityRequired")]
            MenuItemIngredient menuItemIngredient)
        {
            // Validate Quantity
            if (menuItemIngredient.QuantityRequired <= 0)
            {
                ModelState.AddModelError(
                    "QuantityRequired",
                    "Quantity must be greater than zero.");
            }

            // Prevent Duplicate Relationship
            bool exists =
                await _context.MenuItemIngredients
                    .AnyAsync(m =>
                        m.MenuItemId ==
                            menuItemIngredient.MenuItemId &&
                        m.IngredientId ==
                            menuItemIngredient.IngredientId);

            if (exists)
            {
                ModelState.AddModelError(
                    "",
                    "This ingredient is already assigned to this menu item.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(menuItemIngredient);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["IngredientId"] =
                new SelectList(
                    _context.Ingredients,
                    "IngredientId",
                    "Name",
                    menuItemIngredient.IngredientId);

            ViewData["MenuItemId"] =
                new SelectList(
                    _context.MenuItems,
                    "MenuItemId",
                    "Name",
                    menuItemIngredient.MenuItemId);

            return View(menuItemIngredient);
        }


        // GET: MenuItemIngredients/Edit
        public async Task<IActionResult> Edit(
            int? menuItemId,
            int? ingredientId)
        {
            if (menuItemId == null ||
                ingredientId == null)
            {
                return NotFound();
            }

            var menuItemIngredient =
                await _context.MenuItemIngredients
                    .FirstOrDefaultAsync(m =>
                        m.MenuItemId == menuItemId &&
                        m.IngredientId == ingredientId);

            if (menuItemIngredient == null)
            {
                return NotFound();
            }

            ViewData["IngredientId"] =
                new SelectList(
                    _context.Ingredients,
                    "IngredientId",
                    "Name",
                    menuItemIngredient.IngredientId);

            ViewData["MenuItemId"] =
                new SelectList(
                    _context.MenuItems,
                    "MenuItemId",
                    "Name",
                    menuItemIngredient.MenuItemId);

            return View(menuItemIngredient);
        }


        // POST: MenuItemIngredients/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int menuItemId,
            int ingredientId,
            [Bind(
                "MenuItemId,IngredientId,QuantityRequired")]
            MenuItemIngredient menuItemIngredient)
        {
            if (menuItemId !=
                    menuItemIngredient.MenuItemId ||
                ingredientId !=
                    menuItemIngredient.IngredientId)
            {
                return NotFound();
            }

            var existingItem =
                await _context.MenuItemIngredients
                    .FirstOrDefaultAsync(m =>
                        m.MenuItemId == menuItemId &&
                        m.IngredientId == ingredientId);

            if (existingItem == null)
            {
                return NotFound();
            }

            // Validate Quantity
            if (menuItemIngredient.QuantityRequired <= 0)
            {
                ModelState.AddModelError(
                    "QuantityRequired",
                    "Quantity must be greater than zero.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingItem.QuantityRequired =
                        menuItemIngredient.QuantityRequired;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemIngredientExists(
                        menuItemIngredient.MenuItemId,
                        menuItemIngredient.IngredientId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["IngredientId"] =
                new SelectList(
                    _context.Ingredients,
                    "IngredientId",
                    "Name",
                    menuItemIngredient.IngredientId);

            ViewData["MenuItemId"] =
                new SelectList(
                    _context.MenuItems,
                    "MenuItemId",
                    "Name",
                    menuItemIngredient.MenuItemId);

            return View(menuItemIngredient);
        }


        // GET: MenuItemIngredients/Delete
        public async Task<IActionResult> Delete(
            int? menuItemId,
            int? ingredientId)
        {
            if (menuItemId == null ||
                ingredientId == null)
            {
                return NotFound();
            }

            var menuItemIngredient =
                await _context.MenuItemIngredients
                    .Include(m => m.Ingredient)
                    .Include(m => m.MenuItem)
                    .FirstOrDefaultAsync(m =>
                        m.MenuItemId == menuItemId &&
                        m.IngredientId == ingredientId);

            if (menuItemIngredient == null)
            {
                return NotFound();
            }

            return View(menuItemIngredient);
        }


        // POST: MenuItemIngredients/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int menuItemId,
            int ingredientId)
        {
            var menuItemIngredient =
                await _context.MenuItemIngredients
                    .FirstOrDefaultAsync(m =>
                        m.MenuItemId == menuItemId &&
                        m.IngredientId == ingredientId);

            if (menuItemIngredient == null)
            {
                return NotFound();
            }

            _context.MenuItemIngredients
                .Remove(menuItemIngredient);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool MenuItemIngredientExists(
            int menuItemId,
            int ingredientId)
        {
            return _context.MenuItemIngredients.Any(e =>
                e.MenuItemId == menuItemId &&
                e.IngredientId == ingredientId);
        }
    }
}