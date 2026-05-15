using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderingSystem.Models;
namespace RestaurantOrderingSystem
{
    public class Program
    {


        public static void Main(string[] args)
        {

            //BY Mostafa 
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthorization(options =>
            {
                // Policy ÎÇÕÉ ÈÇáÜ Admin ÝÞØ
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

            });


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor(); // <-- ÃÖÝ åÐÇ ÇáÓØÑ åäÇ

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            // Enable Session Middleware [For Login]
            app.UseSession();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
