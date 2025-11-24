using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GymController : Controller
{
    private readonly ApplicationDbContext _context;

    public GymController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Gym
    public async Task<IActionResult> Index()
    {
        var gyms = await _context.Gyms.ToListAsync();
        return View(gyms);
    }

    // GET: Admin/Gym/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var gym = await _context.Gyms
            .Include(g => g.Trainers)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (gym == null)
            return NotFound();

        return View(gym);
    }

    // GET: Admin/Gym/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Gym/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Gym gym)
    {
        if (ModelState.IsValid)
        {
            _context.Add(gym);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Salon başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(gym);
    }

    // GET: Admin/Gym/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var gym = await _context.Gyms.FindAsync(id);
        if (gym == null)
            return NotFound();

        return View(gym);
    }

    // POST: Admin/Gym/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Gym gym)
    {
        if (id != gym.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(gym);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Salon başarıyla güncellendi!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GymExists(gym.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(gym);
    }

    // GET: Admin/Gym/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var gym = await _context.Gyms
            .FirstOrDefaultAsync(m => m.Id == id);

        if (gym == null)
            return NotFound();

        return View(gym);
    }

    // POST: Admin/Gym/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var gym = await _context.Gyms.FindAsync(id);
        if (gym != null)
        {
            _context.Gyms.Remove(gym);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Salon başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool GymExists(int id)
    {
        return _context.Gyms.Any(e => e.Id == id);
    }
}
