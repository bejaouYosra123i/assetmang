using ITAssetManagement1.Data;
using ITAssetManagement1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using OfficeOpenXml; // For EPPlus

namespace ITAssetManagement1.Controllers
{
    public class InvestmentRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvestmentRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InvestmentRequests
        public async Task<IActionResult> Index(string statusFilter = "All")
        {
            var requests = _context.InvestmentRequests
                .Include(r => r.Items)
                .Include(r => r.CreatedBy)
                .AsQueryable();

            if (statusFilter != "All")
            {
                requests = requests.Where(r => r.Status == statusFilter);
            }

            ViewBag.PendingCount = _context.InvestmentRequests.Count(r => r.Status == "Pending");
            ViewBag.RequestedCount = _context.InvestmentRequests.Count(r => r.Status == "Requested");
            ViewBag.ApprovedCount = _context.InvestmentRequests.Count(r => r.Status == "Approved");
            ViewBag.RejectedCount = _context.InvestmentRequests.Count(r => r.Status == "Rejected");
            ViewBag.StatusFilter = statusFilter; // Pass the current filter to the view

            return View(await requests.ToListAsync());
        }

        // GET: InvestmentRequests/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var profile = _context.Profiles.FirstOrDefault(p => p.UserId == userId);
            if (profile == null)
            {
                return RedirectToAction("Create", "Profile");
            }

            ViewBag.CurrentUserProfileId = profile.Id;

            var model = new InvestmentRequest
            {
                RequestedDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(7),
                Items = new List<InvestmentItem> { new InvestmentItem() },
                Status = "Requested", // Default status
                TypeOfInvestment = "New",
                Region = "YEL",
                Currency = "EUR"
            };

            return View(model);
        }

        // POST: InvestmentRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User not authenticated. Please log in.");
                return View(new InvestmentRequest());
            }

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                ModelState.AddModelError("", "User profile not found. Please create a profile first.");
                return View(new InvestmentRequest());
            }

            // Create a new InvestmentRequest object
            var investmentRequest = new InvestmentRequest();

            // Log the form data for debugging
            Console.WriteLine("Form data received:");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"{key}: {Request.Form[key]}");
            }

            // Manually bind simple properties
            investmentRequest.Region = Request.Form["Region"];
            investmentRequest.Currency = Request.Form["Currency"];
            investmentRequest.Location = Request.Form["Location"];
            investmentRequest.TypeOfInvestment = Request.Form["TypeOfInvestment"];
            investmentRequest.Justification = Request.Form["Justification"];
            investmentRequest.CreatedById = int.Parse(Request.Form["CreatedById"]);
            investmentRequest.CreationDate = DateTime.Now;

            // Parse RequestedDate and DueDate
            try
            {
                investmentRequest.RequestedDate = DateTime.ParseExact(Request.Form["RequestedDate"], "dd/MM/yyyy HH:mm", null);
            }
            catch (FormatException)
            {
                ModelState.AddModelError("RequestedDate", "Invalid Requested Date format.");
                investmentRequest.RequestedDate = DateTime.Now; // Fallback
            }

            try
            {
                investmentRequest.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            }
            catch (FormatException)
            {
                ModelState.AddModelError("DueDate", "Invalid Due Date format.");
                investmentRequest.DueDate = DateTime.Now.AddDays(7); // Fallback
            }

            // Manually bind Status
            investmentRequest.Status = Request.Form["Status"];
            if (string.IsNullOrEmpty(investmentRequest.Status) || (investmentRequest.Status != "Requested" && investmentRequest.Status != "Approved"))
            {
                investmentRequest.Status = "Requested";
                Console.WriteLine("Status set to Requested in controller");
            }

            // Manually bind Items
            investmentRequest.Items = new List<InvestmentItem>();
            var itemKeys = Request.Form.Keys.Where(k => k.StartsWith("Items[")).Select(k => int.Parse(k.Split('[')[1].Split(']')[0])).Distinct().ToList();
            foreach (var i in itemKeys)
            {
                var item = new InvestmentItem
                {
                    Item = Request.Form[$"Items[{i}].Item"],
                    Description = Request.Form[$"Items[{i}].Description"],
                    Supplier = Request.Form[$"Items[{i}].Supplier"],
                    UnitCost = decimal.TryParse(Request.Form[$"Items[{i}].UnitCost"], out var unitCost) ? unitCost : 0,
                    Shipping = decimal.TryParse(Request.Form[$"Items[{i}].Shipping"], out var shipping) ? shipping : 0,
                    Quantity = int.TryParse(Request.Form[$"Items[{i}].Quantity"], out var quantity) ? quantity : 1
                };

                // Do not set Subtotal and Total directly; they are computed properties
                // Ensure UnitCost, Shipping, and Quantity are valid
                item.UnitCost = item.UnitCost < 0 ? 0 : item.UnitCost;
                item.Shipping = item.Shipping < 0 ? 0 : item.Shipping;
                item.Quantity = item.Quantity <= 0 ? 1 : item.Quantity;

                investmentRequest.Items.Add(item);
            }

            // Calculate the total for the InvestmentRequest
            investmentRequest.Total = investmentRequest.Items.Any() ? investmentRequest.Items.Sum(item => item.Total) : 0;

            // Custom validation for Status
            if (string.IsNullOrEmpty(investmentRequest.Status))
            {
                ModelState.AddModelError("Status", "The Status field is required.");
            }
            else if (investmentRequest.Status != "Requested" && investmentRequest.Status != "Approved")
            {
                ModelState.AddModelError("Status", "The Status must be either 'Requested' or 'Approved'.");
            }

            // Custom validation for Items
            if (investmentRequest.Items != null)
            {
                for (int i = 0; i < investmentRequest.Items.Count; i++)
                {
                    var item = investmentRequest.Items[i];
                    if (string.IsNullOrEmpty(item.Item))
                    {
                        ModelState.AddModelError($"Items[{i}].Item", "The Item field is required.");
                    }
                    if (string.IsNullOrEmpty(item.Description))
                    {
                        ModelState.AddModelError($"Items[{i}].Description", "The Description field is required.");
                    }
                    if (string.IsNullOrEmpty(item.Supplier))
                    {
                        ModelState.AddModelError($"Items[{i}].Supplier", "The Supplier field is required.");
                    }
                }
            }

            // Validate Due Date
            if (investmentRequest.DueDate < DateTime.Now)
            {
                ModelState.AddModelError("DueDate", "The Due Date must be in the future.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(investmentRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Request created with success";
                return RedirectToAction(nameof(Index), new { statusFilter = "Requested" });
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState errors:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            ViewBag.CurrentUserProfileId = profile.Id;
            return View(investmentRequest);
        }

        // GET: InvestmentRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var investmentRequest = await _context.InvestmentRequests
                .Include(r => r.Items)
                .Include(r => r.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (investmentRequest == null)
            {
                return NotFound();
            }

            return View(investmentRequest);
        }

        // GET: InvestmentRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var investmentRequest = await _context.InvestmentRequests
                .Include(r => r.Items)
                .Include(r => r.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (investmentRequest == null)
            {
                return NotFound();
            }

            return View(investmentRequest);
        }

        // POST: InvestmentRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var investmentRequest = await _context.InvestmentRequests
                .Include(r => r.Items)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (investmentRequest == null || id != investmentRequest.Id)
            {
                return NotFound();
            }

            // Manually bind simple properties
            investmentRequest.Region = Request.Form["Region"];
            investmentRequest.Currency = Request.Form["Currency"];
            investmentRequest.Location = Request.Form["Location"];
            investmentRequest.TypeOfInvestment = Request.Form["TypeOfInvestment"];
            investmentRequest.Justification = Request.Form["Justification"];

            try
            {
                investmentRequest.RequestedDate = DateTime.ParseExact(Request.Form["RequestedDate"], "dd/MM/yyyy HH:mm", null);
            }
            catch (FormatException)
            {
                ModelState.AddModelError("RequestedDate", "Invalid Requested Date format.");
            }

            try
            {
                investmentRequest.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            }
            catch (FormatException)
            {
                ModelState.AddModelError("DueDate", "Invalid Due Date format.");
            }

            // Manually bind Status
            investmentRequest.Status = Request.Form["Status"];
            if (string.IsNullOrEmpty(investmentRequest.Status) || (investmentRequest.Status != "Requested" && investmentRequest.Status != "Approved"))
            {
                investmentRequest.Status = "Requested";
            }

            // Manually bind Items
            investmentRequest.Items.Clear(); // Clear existing items
            var itemKeys = Request.Form.Keys.Where(k => k.StartsWith("Items[")).Select(k => int.Parse(k.Split('[')[1].Split(']')[0])).Distinct().ToList();
            foreach (var i in itemKeys)
            {
                var item = new InvestmentItem
                {
                    Item = Request.Form[$"Items[{i}].Item"],
                    Description = Request.Form[$"Items[{i}].Description"],
                    Supplier = Request.Form[$"Items[{i}].Supplier"],
                    UnitCost = decimal.TryParse(Request.Form[$"Items[{i}].UnitCost"], out var unitCost) ? unitCost : 0,
                    Shipping = decimal.TryParse(Request.Form[$"Items[{i}].Shipping"], out var shipping) ? shipping : 0,
                    Quantity = int.TryParse(Request.Form[$"Items[{i}].Quantity"], out var quantity) ? quantity : 1
                };

                // Do not set Subtotal and Total directly; they are computed properties
                item.UnitCost = item.UnitCost < 0 ? 0 : item.UnitCost;
                item.Shipping = item.Shipping < 0 ? 0 : item.Shipping;
                item.Quantity = item.Quantity <= 0 ? 1 : item.Quantity;

                investmentRequest.Items.Add(item);
            }

            // Calculate the total
            investmentRequest.Total = investmentRequest.Items.Any() ? investmentRequest.Items.Sum(item => item.Total) : 0;

            // Custom validation for Status
            if (string.IsNullOrEmpty(investmentRequest.Status))
            {
                ModelState.AddModelError("Status", "The Status field is required.");
            }
            else if (investmentRequest.Status != "Requested" && investmentRequest.Status != "Approved")
            {
                ModelState.AddModelError("Status", "The Status must be either 'Requested' or 'Approved'.");
            }

            // Custom validation for Items
            if (investmentRequest.Items != null)
            {
                for (int i = 0; i < investmentRequest.Items.Count; i++)
                {
                    var item = investmentRequest.Items[i];
                    if (string.IsNullOrEmpty(item.Item))
                    {
                        ModelState.AddModelError($"Items[{i}].Item", "The Item field is required.");
                    }
                    if (string.IsNullOrEmpty(item.Description))
                    {
                        ModelState.AddModelError($"Items[{i}].Description", "The Description field is required.");
                    }
                    if (string.IsNullOrEmpty(item.Supplier))
                    {
                        ModelState.AddModelError($"Items[{i}].Supplier", "The Supplier field is required.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(investmentRequest);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Request updated with success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.InvestmentRequests.Any(e => e.Id == investmentRequest.Id))
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

            return View(investmentRequest);
        }

        // POST: InvestmentRequests/DownloadExcel
        [HttpPost]
        [Obsolete]
        public IActionResult DownloadExcel()
        {
            var investmentRequest = new InvestmentRequest();

            // Manually bind simple properties
            investmentRequest.Region = Request.Form["Region"];
            investmentRequest.Currency = Request.Form["Currency"];
            investmentRequest.Location = Request.Form["Location"];
            investmentRequest.TypeOfInvestment = Request.Form["TypeOfInvestment"];
            investmentRequest.Justification = Request.Form["Justification"];
            try
            {
                investmentRequest.RequestedDate = DateTime.ParseExact(Request.Form["RequestedDate"], "dd/MM/yyyy HH:mm", null);
            }
            catch (FormatException)
            {
                investmentRequest.RequestedDate = DateTime.Now;
            }

            try
            {
                investmentRequest.DueDate = DateTime.Parse(Request.Form["DueDate"]);
            }
            catch (FormatException)
            {
                investmentRequest.DueDate = DateTime.Now.AddDays(7);
            }

            // Manually bind Status
            investmentRequest.Status = Request.Form["Status"];
            if (string.IsNullOrEmpty(investmentRequest.Status) || (investmentRequest.Status != "Requested" && investmentRequest.Status != "Approved"))
            {
                investmentRequest.Status = "Requested";
                Console.WriteLine("Status set to Requested in DownloadExcel");
            }

            // Manually bind Items
            investmentRequest.Items = new List<InvestmentItem>();
            var itemKeys = Request.Form.Keys.Where(k => k.StartsWith("Items[")).Select(k => int.Parse(k.Split('[')[1].Split(']')[0])).Distinct().ToList();
            foreach (var i in itemKeys)
            {
                var item = new InvestmentItem
                {
                    Item = Request.Form[$"Items[{i}].Item"],
                    Description = Request.Form[$"Items[{i}].Description"],
                    Supplier = Request.Form[$"Items[{i}].Supplier"],
                    UnitCost = decimal.TryParse(Request.Form[$"Items[{i}].UnitCost"], out var unitCost) ? unitCost : 0,
                    Shipping = decimal.TryParse(Request.Form[$"Items[{i}].Shipping"], out var shipping) ? shipping : 0,
                    Quantity = int.TryParse(Request.Form[$"Items[{i}].Quantity"], out var quantity) ? quantity : 1
                };

                // Do not set Subtotal and Total directly; they are computed properties
                item.UnitCost = item.UnitCost < 0 ? 0 : item.UnitCost;
                item.Shipping = item.Shipping < 0 ? 0 : item.Shipping;
                item.Quantity = item.Quantity <= 0 ? 1 : item.Quantity;

                investmentRequest.Items.Add(item);
            }

            // Calculate the total
            investmentRequest.Total = investmentRequest.Items.Any() ? investmentRequest.Items.Sum(item => item.Total) : 0;

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Investment Request");

                // Add headers for the main details
                worksheet.Cells[1, 1].Value = "Region";
                worksheet.Cells[1, 2].Value = investmentRequest.Region;
                worksheet.Cells[2, 1].Value = "Currency";
                worksheet.Cells[2, 2].Value = investmentRequest.Currency;
                worksheet.Cells[3, 1].Value = "Location";
                worksheet.Cells[3, 2].Value = investmentRequest.Location;
                worksheet.Cells[4, 1].Value = "Type of Investment";
                worksheet.Cells[4, 2].Value = investmentRequest.TypeOfInvestment;
                worksheet.Cells[5, 1].Value = "Justification";
                worksheet.Cells[5, 2].Value = investmentRequest.Justification;
                worksheet.Cells[6, 1].Value = "Requested Date";
                worksheet.Cells[6, 2].Value = investmentRequest.RequestedDate.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[7, 1].Value = "Due Date";
                worksheet.Cells[7, 2].Value = investmentRequest.DueDate.ToString("dd/MM/yyyy");
                worksheet.Cells[8, 1].Value = "Total";
                worksheet.Cells[8, 2].Value = investmentRequest.Total.ToString("F2");

                int row = 10;

                // Add headers for the items table
                worksheet.Cells[row, 1].Value = "No.";
                worksheet.Cells[row, 2].Value = "Item";
                worksheet.Cells[row, 3].Value = "Description";
                worksheet.Cells[row, 4].Value = "Supplier";
                worksheet.Cells[row, 5].Value = "Unit Cost";
                worksheet.Cells[row, 6].Value = "Shipping";
                worksheet.Cells[row, 7].Value = "Subtotal";
                worksheet.Cells[row, 8].Value = "Quantity";
                worksheet.Cells[row, 9].Value = "Total";

                int itemNumber = 1;
                foreach (var item in investmentRequest.Items)
                {
                    row++;
                    worksheet.Cells[row, 1].Value = itemNumber;
                    worksheet.Cells[row, 2].Value = item.Item;
                    worksheet.Cells[row, 3].Value = item.Description;
                    worksheet.Cells[row, 4].Value = item.Supplier;
                    worksheet.Cells[row, 5].Value = item.UnitCost;
                    worksheet.Cells[row, 6].Value = item.Shipping;
                    worksheet.Cells[row, 7].Value = item.Subtotal;
                    worksheet.Cells[row, 8].Value = item.Quantity;
                    worksheet.Cells[row, 9].Value = item.Total;
                    itemNumber++;
                }

                worksheet.Cells.AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ITInvestmentForm.xlsx");
            }
        }
    }
}