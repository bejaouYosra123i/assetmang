using ITAssetManagement1.Data;
using ITAssetManagement1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITAssetManagement1.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Seed roles
                string[] roleNames = { "Admin", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (!roleResult.Succeeded)
                        {
                            logger.LogError("Failed to create role {RoleName}: {Errors}",
                                roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                            throw new Exception($"Failed to create role {roleName}");
                        }
                        logger.LogInformation("Created role {RoleName}", roleName);
                    }
                }

                // Seed admin user
                var adminEmail = "admin@example.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    var userResult = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (!userResult.Succeeded)
                    {
                        logger.LogError("Failed to create admin user: {Errors}",
                            string.Join(", ", userResult.Errors.Select(e => e.Description)));
                        throw new Exception("Failed to create admin user");
                    }
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Created admin user {Email}", adminEmail);
                }

                // Seed test user
                var testEmail = "testuser@example.com";
                var testUser = await userManager.FindByEmailAsync(testEmail);
                if (testUser == null)
                {
                    testUser = new IdentityUser
                    {
                        UserName = testEmail,
                        Email = testEmail,
                        EmailConfirmed = true
                    };
                    var userResult = await userManager.CreateAsync(testUser, "Test@123");
                    if (!userResult.Succeeded)
                    {
                        logger.LogError("Failed to create test user: {Errors}",
                            string.Join(", ", userResult.Errors.Select(e => e.Description)));
                        throw new Exception("Failed to create test user");
                    }
                    await userManager.AddToRoleAsync(testUser, "User");
                    logger.LogInformation("Created test user {Email}", testEmail);
                }

                
                
                // Seed investment data
                if (!await context.InvestmentRequests.AnyAsync())
                {
                    var investmentItems = new List<InvestmentItem>
                    {
                        new InvestmentItem
                        {
                            Item = "KNIPEX VDE Seitenschneider mit Facette",
                            Description = "180mm",
                            Supplier = "IT-HAUS",
                            UnitCost = 26.5m,
                            Quantity = 1,
                            Shipping = 0m,
                            Items = new List<InvestmentItem>()
                        },
                        new InvestmentItem
                        {
                            Item = "Stanley Schraubendreher 57Stück",
                            Description = "",
                            Supplier = "IT-HAUS",
                            UnitCost = 45m,
                            Quantity = 1,
                            Shipping = 0m,
                            Items = new List<InvestmentItem>()
                        },
                        new InvestmentItem
                        {
                            Item = "LogiLink Präsentations-Fernsteuerung",
                            Description = "3 Tasten",
                            Supplier = "IT-HAUS",
                            UnitCost = 13.5m,
                            Quantity = 5,
                            Shipping = 0m,
                            Items = new List<InvestmentItem>()
                        },
                        new InvestmentItem
                        {
                            Item = "DIGITUS Ultra Slim HDMI Splitter",
                            Description = "4x HDMI Desktop",
                            Supplier = "IT-HAUS",
                            UnitCost = 55m,
                            Quantity = 5,
                            Shipping = 0m,
                            Items = new List<InvestmentItem>()
                        },
                        new InvestmentItem
                        {
                            Item = "DELL USB-C AC Adapter E5 Kit",
                            Description = "Netzteil 65 Watt",
                            Supplier = "IT-HAUS",
                            UnitCost = 38.5m,
                            Quantity = 8,
                            Shipping = 0m,
                            Items = new List<InvestmentItem>()
                        },
                        new InvestmentItem
                        {
                            Item = "Dell EcoLoop Urban CP4523G",
                            Description = "Notebook-Rucksack",
                            Supplier = "IT-HAUS",
                            UnitCost = 39m,
                            Quantity = 7,
                            Shipping = 0m,
                            Items = new List<InvestmentItem>()
                        }
                    };

                    var investmentRequest = new InvestmentRequest
                    {
                        Region = "YEL",
                        Currency = "EUR",
                        Location = "",
                        TypeOfInvestment = "New",
                        Justification = "replacement for old ones",
                        RequestedDate = new DateTime(2025, 4, 23),
                        DueDate = null,
                        Status = "Pending",
                        Observation = "",
                        Shipping = 217.5m,
                        Description = "", // Set a default non-null value for Description
                        Item = "Investment Request", // Set a default value for Item (also inherited from InvestmentItem)
                        Supplier = "N/A", // Set a default value for Supplier (also inherited from InvestmentItem)
                        UnitCost = 0m, // Set a default value for UnitCost (also inherited from InvestmentItem)
                        Quantity = 1, // Set a default value for Quantity (also inherited from InvestmentItem)
                        Items = new List<InvestmentItem>()
                    };

                    // Add and save the InvestmentRequest first to generate its Id
                    context.InvestmentRequests.Add(investmentRequest);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded investment request data");

                    // Now set the InvestmentRequestId for each child InvestmentItem and add them
                    
                    // Save the child InvestmentItems
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded investment items data");
                }

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding database.");
                throw;
            }
        }
    }
}