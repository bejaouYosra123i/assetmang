using ITAssetManagement1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ITAssetManagement1.Data
{
    public static class SeedData
    {
        // Initialize the database with default data (roles, users, profiles, and investment requests)
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // Create a new instance of the database context
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Get the role and user managers
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Seed roles (admin and user)
            string[] roleNames = { "admin", "user" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed an admin user
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    // Assign the admin role to the user
                    await userManager.AddToRoleAsync(adminUser, "admin");
                }
                else
                {
                    // Log any errors if user creation fails
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating admin user: {error.Description}");
                    }
                    return;
                }
            }

            // Seed Profiles and Investment Requests
            if (context.Profiles.Any())
            {
                return; // Database has been seeded
            }

            // Create a profile for the admin user
            var profile = new Profile
            {
                UserId = adminUser.Id,
                FirstName = "John",
                LastName = "Doe",
                Email = adminEmail, // Match the email from the admin user
                Department = "IT Department"
            };
            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            // Seed a sample investment request
            var request = new InvestmentRequest
            {
                Region = "YEL",
                Currency = "EUR",
                Location = "",
                TypeOfInvestment = "New",
                Justification = "replacement for old ones",
                RequestedDate = new DateTime(2024, 9, 16),
                DueDate = DateTime.Now.AddMonths(1),
                CreatedById = profile.Id,
                CreationDate = DateTime.Now,
                Status = "Pending",
                Items = new List<InvestmentItem>
                {
                    new InvestmentItem
                    {
                        Item = "Other",
                        Description = "KNIPEX VDE Seitenschneider mit Facette 180mm",
                        Supplier = "IT-HAUS",
                        UnitCost = 26.50m,
                        Shipping = 0.00m,
                        Quantity = 1
                    },
                    // ... (other items remain the same)
                }
            };

            context.InvestmentRequests.Add(request);
            await context.SaveChangesAsync();
        }
    }
}