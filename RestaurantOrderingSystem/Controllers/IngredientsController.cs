using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;

namespace RestaurantOrderingSystem.Controllers
{
    public class IngredientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IngredientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get Current Logged User Id From Session
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        // Get Current User Role From Session
        private string? CurrentUserRole => HttpContext.Session.GetString("Role");

        // Check Login Before Any Action
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            if (CurrentUserId == null)
            {
                context.Result =
                    RedirectToAction("Login", "Account");
            }

            base.OnActionExecuting(context);
        }

        // Check Admin Access
        private IActionResult CheckAdmin()
        {
            if (CurrentUserRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return null;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index()
        {
            var adminCheck = CheckAdmin();

            if (adminCheck != null)
            {
                return adminCheck;
            }

            return View(await _context.Ingredients.ToListAsync());
        }

        // GET: Ingredients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var adminCheck = CheckAdmin();

            if (adminCheck != null)
            {
                return adminCheck;
            }

            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(m => m.IngredientId == id);

            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // GET: Ingredients/Create
        public IActionResult Create()
        {
            var adminCheck = CheckAdmin();

            if (adminCheck != null)
            {
                return adminCheck;
            }

            return View();
        }

        // POST: Ingredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("IngredientId,Name,Description,QuantityInStock,Unit,UnitPrice")]
            Ingredient ingredient)
        {
            var adminCheck = CheckAdmin();

            if (adminCheck != null)
            {
                return adminCheck;
            }

            if (ModelState.IsValid)
            {
                _context.Add(ingredient);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(ingredient);
        }

        // GET: Ingredients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var adminCheck = CheckAdmin();

            if (adminCheck != null)
            {
                return adminCheck;
            }

            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients
                .FindAsync(id);

            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // POST: Ingredients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IngredientId,Name,Description,QuantityInStock,Unit,UnitPrice")]
            Ingredient ingredient)
        {
            var adminCheck = CheckAdmin();

            if (adminCheck != null)
            {
                return adminCheck;
            }

            if (id != ingredient.IngredientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingIngredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.IngredientId == id);

                    if (existingIngredient == null)
                    {
                        return NotFound();
                    }

                    existingIngredient.Name = ingredient.Name;
                    existingIngredient.Description = ingredient.Description;
                    existingIngredient.QuantityInStock = ingredient.QuantityInStock;
                    existingIngredient.Unit = ingredient.Unit;
                    existingIngredient.UnitPrice = ingredient.UnitPrice;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IngredientExists(
                        ingredient.IngredientId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(ingredient);
        }

        // GET: Ingredients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var adminCheck = CheckAdmin();

            if (adminCheck != null)
            {
                return adminCheck;
            }

            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(m => m.IngredientId == id);

            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // POST: Ingredients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminCheck = CheckAdmin();

            if (adminCheck != null)
            {
                return adminCheck;
            }

            var ingredient = await _context.Ingredients
                .FindAsync(id);

            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IngredientExists(int id)
        {
            return _context.Ingredients.Any(e => e.IngredientId == id);
        }
    }
}

/*
================= Modifications =================

1- Added Session-Based Authentication
- CurrentUserId
- CurrentUserRole

2- Added Automatic Login Protection
- Any User Without Session
  Redirected To Login Page

3- Added Admin Authorization
- Only Admin Can Access Ingredients

4- Added AccessDenied Redirection
- Non Admin Users Redirected
  To Friendly AccessDenied Page

5- Added Secure Update Logic
- Removed _context.Update()
- Added Controlled Property Update

6- Added Extra Null Checking
- Ingredient Validation Before Edit/Delete

7- Improved Security & Business Logic
- Ingredients Management Is Admin Only

=================================================
*/