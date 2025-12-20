using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TrainerAvailabilityController : Controller
{
    private readonly ApplicationDbContext _context;

    public TrainerAvailabilityController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/TrainerAvailability
    public async Task<IActionResult> Index()
    {
        var availabilities = await _context.TrainerAvailabilities
            .Include(ta => ta.Trainer)
                .ThenInclude(t => t.Gym)
            .OrderBy(ta => ta.Trainer.FullName)
            .ThenBy(ta => ta.DayOfWeek)
            .ThenBy(ta => ta.StartTime)
            .ToListAsync();
        return View(availabilities);
    }

    // GET: Admin/TrainerAvailability/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var availability = await _context.TrainerAvailabilities
            .Include(ta => ta.Trainer)
                .ThenInclude(t => t.Gym)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (availability == null)
            return NotFound();

        return View(availability);
    }

    // GET: Admin/TrainerAvailability/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Trainers = new SelectList(
            await _context.Trainers.Include(t => t.Gym).ToListAsync(), 
            "Id", 
            "FullName");
        ViewBag.DaysOfWeek = GetDaysOfWeek();
        return View();
    }

    // POST: Admin/TrainerAvailability/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainerAvailability availability)
    {
        if (ModelState.IsValid)
        {
            _context.Add(availability);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Müsaitlik başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Trainers = new SelectList(
            await _context.Trainers.Include(t => t.Gym).ToListAsync(), 
            "Id", 
            "FullName", 
            availability.TrainerId);
        ViewBag.DaysOfWeek = GetDaysOfWeek();
        return View(availability);
    }

    // GET: Admin/TrainerAvailability/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var availability = await _context.TrainerAvailabilities.FindAsync(id);
        if (availability == null)
            return NotFound();

        ViewBag.Trainers = new SelectList(
            await _context.Trainers.Include(t => t.Gym).ToListAsync(), 
            "Id", 
            "FullName", 
            availability.TrainerId);
        ViewBag.DaysOfWeek = GetDaysOfWeek();
        return View(availability);
    }

    // POST: Admin/TrainerAvailability/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TrainerAvailability availability)
    {
        if (id != availability.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(availability);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Müsaitlik başarıyla güncellendi!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AvailabilityExists(availability.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Trainers = new SelectList(
            await _context.Trainers.Include(t => t.Gym).ToListAsync(), 
            "Id", 
            "FullName", 
            availability.TrainerId);
        ViewBag.DaysOfWeek = GetDaysOfWeek();
        return View(availability);
    }

    // GET: Admin/TrainerAvailability/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var availability = await _context.TrainerAvailabilities
            .Include(ta => ta.Trainer)
                .ThenInclude(t => t.Gym)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (availability == null)
            return NotFound();

        return View(availability);
    }

    // POST: Admin/TrainerAvailability/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var availability = await _context.TrainerAvailabilities.FindAsync(id);
        if (availability != null)
        {
            _context.TrainerAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Müsaitlik başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool AvailabilityExists(int id)
    {
        return _context.TrainerAvailabilities.Any(e => e.Id == id);
    }

    // gun isimleri
    private SelectList GetDaysOfWeek()
    {
        var days = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "Pazar" },
            new SelectListItem { Value = "1", Text = "Pazartesi" },
            new SelectListItem { Value = "2", Text = "Salı" },
            new SelectListItem { Value = "3", Text = "Çarşamba" },
            new SelectListItem { Value = "4", Text = "Perşembe" },
            new SelectListItem { Value = "5", Text = "Cuma" },
            new SelectListItem { Value = "6", Text = "Cumartesi" }
        };
        return new SelectList(days, "Value", "Text");
    }
}
