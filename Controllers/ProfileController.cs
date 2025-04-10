using ITAssetManagement1.Data;
using ITAssetManagement1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ITAssetManagement1.Controllers
{
    public class ProfileController : Controller
    {
        // Dependency injection for database context, user manager, and logger
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<ProfileController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Profile/Index
        // Displays the profile page for the logged-in user
        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // If no user is logged in, redirect to the login page
                _logger.LogWarning("No user is logged in.");
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Find the user's profile in the database using their UserId
            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                // If no profile exists, create a default one
                profile = new Profile
                {
                    UserId = user.Id,
                    FirstName = "Unknown", // Default value, can be updated by the user
                    LastName = "User",
                    Email = user.Email, // Use the email from the logged-in user
                    Department = "Not Specified"
                };
                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created a new profile for user {user.Email}.");
            }

            // Pass the profile to the view
            return View(profile);
        }

        // POST: Profile/Index
        // Handles form submission to update the user's profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Profile profile)
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("No user is logged in.");
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Log the incoming profile data for debugging
            _logger.LogInformation($"Received profile data: Id={profile.Id}, FirstName={profile.FirstName}, LastName={profile.LastName}, Email={profile.Email}, Department={profile.Department}");

            // Ensure the profile belongs to the logged-in user
            var existingProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.Id == profile.Id && p.UserId == user.Id);

            if (existingProfile == null)
            {
                _logger.LogWarning($"Profile not found for Id={profile.Id} and UserId={user.Id}.");
                ModelState.AddModelError("", "Profile not found or you do not have permission to edit this profile.");
                return View(profile);
            }

            // Log the ModelState errors if validation fails
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning($"ModelState is invalid. Errors: {string.Join(", ", errors)}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the existing profile with the new values
                    existingProfile.FirstName = profile.FirstName;
                    existingProfile.LastName = profile.LastName;
                    existingProfile.Email = user.Email; // Keep email in sync with IdentityUser
                    existingProfile.Department = profile.Department;

                    // Mark the entity as modified
                    _context.Update(existingProfile);

                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Profile updated successfully for user {user.Email}.");
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log any database errors
                    _logger.LogError(ex, "Error saving profile changes to the database.");
                    ModelState.AddModelError("", "An error occurred while saving your changes. Please try again.");
                }
            }

            // If the model state is invalid, return the form with errors
            return View(profile);
        }
    }
    }
