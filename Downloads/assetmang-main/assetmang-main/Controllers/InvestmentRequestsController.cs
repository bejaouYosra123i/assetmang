using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITAssetManagement1.Data;
using ITAssetManagement1.Models;

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
        public async Task<IActionResult> Index()
        {
            return View(await _context.InvestmentRequests.ToListAsync());
        }

        // GET: InvestmentRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var investmentRequest = await _context.InvestmentRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (investmentRequest == null)
            {
                return NotFound();
            }

            return View(investmentRequest);
        }

        // GET: InvestmentRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: InvestmentRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Region,Currency,Location,TypeOfInvestment,Justification,RequestedDate,DueDate,Status,Observation,Id,Item,Description,Supplier,Shipping,UnitCost,Quantity")] InvestmentRequest investmentRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(investmentRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

            var investmentRequest = await _context.InvestmentRequests.FindAsync(id);
            if (investmentRequest == null)
            {
                return NotFound();
            }
            return View(investmentRequest);
        }

        // POST: InvestmentRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Region,Currency,Location,TypeOfInvestment,Justification,RequestedDate,DueDate,Status,Observation,Id,Item,Description,Supplier,Shipping,UnitCost,Quantity")] InvestmentRequest investmentRequest)
        {
            if (id != investmentRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(investmentRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvestmentRequestExists(investmentRequest.Id))
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

        // GET: InvestmentRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var investmentRequest = await _context.InvestmentRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (investmentRequest == null)
            {
                return NotFound();
            }

            return View(investmentRequest);
        }

        // POST: InvestmentRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var investmentRequest = await _context.InvestmentRequests.FindAsync(id);
            if (investmentRequest != null)
            {
                _context.InvestmentRequests.Remove(investmentRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InvestmentRequestExists(int id)
        {
            return _context.InvestmentRequests.Any(e => e.Id == id);
        }
    }
}
