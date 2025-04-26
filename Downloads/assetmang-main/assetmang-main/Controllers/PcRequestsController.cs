namespace ITAssetManagement1.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using ITAssetManagement1.Data;
    using ITAssetManagement1.Models;
    using Microsoft.AspNetCore.Identity;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    public class PcRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<PcRequestsController> _logger;

        public PcRequestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<PcRequestsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: PcRequests
        public async Task<IActionResult> Index()
        {
            var pcRequests = await _context.PcRequests
                .Include(p => p.Validations)
                .Include(p => p.Requester)
                .ToListAsync();
            _logger.LogInformation("Retrieved {Count} PcRequests", pcRequests.Count);
            return View(pcRequests);
        }

        // GET: PcRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcRequest = await _context.PcRequests
                .Include(p => p.Validations)
                .Include(p => p.Requester)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pcRequest == null)
            {
                return NotFound();
            }

            return View(pcRequest);
        }

        // GET: PcRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PcRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PcType,NeedDescription,RequesterName,Department,Fonction,RequestDate")] PcRequest pcRequest)
        {
            // Log the raw form data
            var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            _logger.LogInformation("Raw form data: {@FormData}", formData);

            // Log the bound PcRequest object
            _logger.LogInformation("Bound PcRequest: {@PcRequest}", pcRequest);

            // Set the RequesterId to the currently logged-in user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Unauthorized access: No user logged in.");
                return Unauthorized();
            }
            pcRequest.RequesterId = currentUser.Id;
            _logger.LogInformation("Set RequesterId to {RequesterId}", pcRequest.RequesterId);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(pcRequest);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("PcRequest created successfully with ID {Id}", pcRequest.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving PcRequest: {Message}", ex.Message);
                    ModelState.AddModelError("", "An error occurred while saving the request. Please try again.");
                }
            }
            else
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    _logger.LogWarning("Validation Error: {Error}", error);
                }
            }
            return View(pcRequest);
        }

        // GET: PcRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcRequest = await _context.PcRequests.FindAsync(id);
            if (pcRequest == null)
            {
                return NotFound();
            }
            return View(pcRequest);
        }

        // POST: PcRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PcType,NeedDescription,Id,RequesterName,Department,Fonction,RequestDate,RequesterId")] PcRequest pcRequest)
        {
            if (id != pcRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pcRequest);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("PcRequest updated successfully with ID {Id}", pcRequest.Id);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error updating PcRequest with ID {Id}", pcRequest.Id);
                    if (!PcRequestExists(pcRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pcRequest);
        }

        // GET: PcRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcRequest = await _context.PcRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pcRequest == null)
            {
                return NotFound();
            }

            return View(pcRequest);
        }

        // POST: PcRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pcRequest = await _context.PcRequests.FindAsync(id);
            if (pcRequest != null)
            {
                _context.PcRequests.Remove(pcRequest);
                await _context.SaveChangesAsync();
                _logger.LogInformation("PcRequest deleted successfully with ID {Id}", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PcRequestExists(int id)
        {
            return _context.PcRequests.Any(e => e.Id == id);
        }

        // Debug action to inspect the database
        public IActionResult Debug()
        {
            var allRequests = _context.ITRequests.ToList();
            var pcRequests = _context.PcRequests.ToList();
            var users = _context.Users.ToList();
            return Json(new { AllRequests = allRequests, PcRequests = pcRequests, Users = users });
        }
    }
}