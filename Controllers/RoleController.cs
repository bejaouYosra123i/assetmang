using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ITAssetManagement1.Controllers
{
    [Authorize(Roles = "admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View(new IdentityRole());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(IdentityRole role)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please provide a valid role name.");
                return View(role);
            }

            if (await _roleManager.RoleExistsAsync(role.Name))
            {
                ModelState.AddModelError("", "This role already exists.");
                return View(role);
            }

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role {role.Name} created successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(role);
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            var users = _userManager.Users.ToList();
            ViewBag.RoleId = roleId;
            ViewBag.RoleName = role.Name;
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string roleId, string userId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if the user is already in the role
            if (await _userManager.IsInRoleAsync(user, role.Name))
            {
                TempData["Error"] = $"User {user.Email} is already in the role {role.Name}.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role {role.Name} assigned to {user.Email} successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            var users = _userManager.Users.ToList();
            ViewBag.RoleId = roleId;
            ViewBag.RoleName = role.Name;
            ViewBag.Users = users;
            return View();
        }
    }
}