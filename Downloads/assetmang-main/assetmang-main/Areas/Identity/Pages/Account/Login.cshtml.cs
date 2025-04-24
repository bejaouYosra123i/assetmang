using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ITAssetManagement1.Areas.Identity.Pages.Account
{
    // Allow anonymous access to the login page (users don't need to be logged in to access it)
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        // Dependency injection for SignInManager, which handles user sign-in
        private readonly SignInManager<IdentityUser> _signInManager;
        // Dependency injection for ILogger to log events
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        // BindProperty attribute binds form data to this property on POST
        [BindProperty]
        public InputModel Input { get; set; }

        // List of external login providers (e.g., Google, Microsoft)
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = [];

        // The URL to redirect to after login
        public string ReturnUrl { get; set; }

        // TempData to store error messages across requests
        [TempData]
        public string ErrorMessage { get; set; }

        // Input model for the login form
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        // GET: Handles the initial page load for the login page
        public async Task OnGetAsync(string? returnUrl = null)
        {
            // Display any error message stored in TempData
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Default return URL to the homepage if none is provided
            returnUrl ??= Url.Content("~/");

            // Clear any existing external login cookies to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Get the list of external login providers
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Set the return URL for use in the view
            ReturnUrl = returnUrl;
        }

        // POST: Handles form submission when the user tries to log in
        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // Default return URL to the homepage if none is provided
            returnUrl ??= Url.Content("~/");

            // Check if the form data is valid (e.g., email and password are provided)
            if (ModelState.IsValid)
            {
                // Attempt to sign in the user with the provided email and password
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Login successful
                    _logger.LogInformation("User logged in.");
                    TempData["SuccessMessage"] = $"Login successful! Welcome, {Input.Email}!";
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    // If two-factor authentication is required, redirect to the 2FA page
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    // If the user is locked out, redirect to the lockout page
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    // If login fails (e.g., wrong email/password), show an error
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    // Re-populate external logins in case the page is re-rendered
                    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                    return Page();
                }
            }

            // If the model state is invalid, re-populate external logins and re-render the page
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return Page();
        }
    }
}