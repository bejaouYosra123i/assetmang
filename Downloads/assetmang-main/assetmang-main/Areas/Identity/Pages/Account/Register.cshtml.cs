using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using ITAssetManagement1.Data;
using ITAssetManagement1.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ITAssetManagement1.Areas.Identity.Pages.Account
{
    
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ApplicationDbContext _context; // Ajout de ApplicationDbContext pour sauvegarder le profil

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext context) // Injection de ApplicationDbContext
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = [];

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Department")]
            public string Department { get; set; }

            [Display(Name = "Profile Image")]
            public IFormFile ProfileImage { get; set; } // Propriété pour l'image uploadée
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Assigner un rôle par défaut à l'utilisateur (par exemple, "user")
                    await _userManager.AddToRoleAsync(user, "user");

                    // Gérer l'upload de l'image
                    string imagePath = null;
                    if (Input.ProfileImage != null && Input.ProfileImage.Length > 0)
                    {
                        // Définir le chemin pour sauvegarder l'image (par exemple, wwwroot/images/profiles/)
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Générer un nom de fichier unique pour l'image
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.ProfileImage.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Sauvegarder l'image sur le serveur
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Input.ProfileImage.CopyToAsync(fileStream);
                        }

                        // Stocker le chemin relatif de l'image
                        imagePath = $"/images/profiles/{uniqueFileName}";
                    }

                    // Créer une entrée Profile pour le nouvel utilisateur
                    var profile = new Profile
                    {
                        UserId = user.Id,
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        Email = Input.Email,
                        Department = Input.Department,
                        ImagePath = imagePath // Sauvegarder le chemin de l'image
                    };

                    _context.Profiles.Add(profile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"User {Input.Email} added successfully!";
                    return RedirectToAction("Index", "Role", new { area = "" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}