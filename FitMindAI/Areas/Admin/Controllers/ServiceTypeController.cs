using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ServiceTypeController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServiceTypeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/ServiceType
    public async Task<IActionResult> Index()
    {
        var serviceTypes = await _context.ServiceTypes.ToListAsync();
        return View(serviceTypes);
    }

    // GET: Admin/ServiceType/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var serviceType = await _context.ServiceTypes
            .Include(s => s.TrainerServices)
                .ThenInclude(ts => ts.Trainer)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (serviceType == null)
            return NotFound();

        return View(serviceType);
    }

    // GET: Admin/ServiceType/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/ServiceType/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceType serviceType)
    {
        if (ModelState.IsValid)
        {
            _context.Add(serviceType);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Hizmet başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(serviceType);
    }

    // GET: Admin/ServiceType/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var serviceType = await _context.ServiceTypes.FindAsync(id);
        if (serviceType == null)
            return NotFound();

        return View(serviceType);
    }

    // POST: Admin/ServiceType/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceType serviceType)
    {
        if (id != serviceType.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(serviceType);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hizmet başarıyla güncellendi!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceTypeExists(serviceType.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(serviceType);
    }

    // GET: Admin/ServiceType/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var serviceType = await _context.ServiceTypes
            .FirstOrDefaultAsync(m => m.Id == id);

        if (serviceType == null)
            return NotFound();

        return View(serviceType);
    }

    // POST: Admin/ServiceType/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(id);
        if (serviceType != null)
        {
            _context.ServiceTypes.Remove(serviceType);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Hizmet başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool ServiceTypeExists(int id)
    {
        return _context.ServiceTypes.Any(e => e.Id == id);
    }
}
