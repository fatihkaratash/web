using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SpecialtyController : Controller
{
    private readonly ApplicationDbContext _context;

    public SpecialtyController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Specialty
    public async Task<IActionResult> Index()
    {
        var specialties = await _context.Specialties.ToListAsync();
        return View(specialties);
    }

    // GET: Admin/Specialty/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var specialty = await _context.Specialties
            .Include(s => s.TrainerSpecialties)
                .ThenInclude(ts => ts.Trainer)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (specialty == null)
            return NotFound();

        return View(specialty);
    }

    // GET: Admin/Specialty/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Specialty/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Specialty specialty)
    {
        if (ModelState.IsValid)
        {
            _context.Add(specialty);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Uzmanlık başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(specialty);
    }

    // GET: Admin/Specialty/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var specialty = await _context.Specialties.FindAsync(id);
        if (specialty == null)
            return NotFound();

        return View(specialty);
    }

    // POST: Admin/Specialty/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Specialty specialty)
    {
        if (id != specialty.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(specialty);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Uzmanlık başarıyla güncellendi!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpecialtyExists(specialty.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(specialty);
    }

    // GET: Admin/Specialty/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var specialty = await _context.Specialties
            .FirstOrDefaultAsync(m => m.Id == id);

        if (specialty == null)
            return NotFound();

        return View(specialty);
    }

    // POST: Admin/Specialty/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var specialty = await _context.Specialties.FindAsync(id);
        if (specialty != null)
        {
            _context.Specialties.Remove(specialty);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Uzmanlık başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool SpecialtyExists(int id)
    {
        return _context.Specialties.Any(e => e.Id == id);
    }
}
