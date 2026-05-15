using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RestaurantOrderingSystem.Models
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<MenuItemIngredient> MenuItemIngredients { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MenuItemIngredient - Composite Key + Relationships
            modelBuilder.Entity<MenuItemIngredient>()
                .HasKey(mi => new { mi.MenuItemId, mi.IngredientId });

            modelBuilder.Entity<MenuItemIngredient>()
                .HasOne(mi => mi.MenuItem)
              .WithMany(m => m.MenuItemIngredients)
                .HasForeignKey(mi => mi.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MenuItemIngredient>()
                .HasOne(mi => mi.Ingredient)
                .WithMany(i => i.MenuItemIngredients)
                .HasForeignKey(mi => mi.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order → OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(oi => oi.MenuItemId);

            // Order → Feedback (One-to-One Optional)
            modelBuilder.Entity<Feedback>()
       .HasOne(f => f.Order)
       .WithMany(o => o.Feedbacks)
       .HasForeignKey(f => f.OrderId)
       .OnDelete(DeleteBehavior.Cascade);

            // User → Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)           // أضف Navigation في Order
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId); // أضف UserId في Order
        }


    }
}