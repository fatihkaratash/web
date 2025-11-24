using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TrainerController : Controller
{
    private readonly ApplicationDbContext _context;

    public TrainerController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Trainer
    public async Task<IActionResult> Index(int? gymId)
    {
        var query = _context.Trainers.Include(t => t.Gym).AsQueryable();
        
        if (gymId.HasValue)
        {
            query = query.Where(t => t.GymId == gymId.Value);
            var gym = await _context.Gyms.FindAsync(gymId.Value);
            ViewBag.GymFilter = gym?.Name;
        }
        
        var trainers = await query.OrderBy(t => t.FullName).ToListAsync();
        return View(trainers);
    }

    // GET: Admin/Trainer/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var trainer = await _context.Trainers
            .Include(t => t.Gym)
            .Include(t => t.TrainerSpecialties)
                .ThenInclude(ts => ts.Specialty)
            .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.ServiceType)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (trainer == null)
            return NotFound();

        return View(trainer);
    }

    // GET: Admin/Trainer/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Gyms = new SelectList(await _context.Gyms.ToListAsync(), "Id", "Name");
        ViewBag.Specialties = await _context.Specialties.ToListAsync();
        ViewBag.ServiceTypes = await _context.ServiceTypes.ToListAsync();
        return View();
    }

    // POST: Admin/Trainer/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Trainer trainer, int[] selectedSpecialties, int[] selectedServices)
    {
        // Navigation property'leri ModelState'den kaldır (sadece ID kullanıyoruz)
        ModelState.Remove("Gym");
        ModelState.Remove("TrainerSpecialties");
        ModelState.Remove("TrainerServices");
        ModelState.Remove("Availabilities");
        ModelState.Remove("Appointments");
        
        // ModelState hatalarını logla (debug için)
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["ErrorMessage"] = "Hata: " + string.Join(", ", errors);
        }
        
        if (ModelState.IsValid)
        {
            _context.Add(trainer);
            await _context.SaveChangesAsync();

            // uzmanliklar ekle
            if (selectedSpecialties != null)
            {
                foreach (var specialtyId in selectedSpecialties)
                {
                    _context.TrainerSpecialties.Add(new TrainerSpecialty
                    {
                        TrainerId = trainer.Id,
                        SpecialtyId = specialtyId
                    });
                }
            }

            // hizmetler ekle
            if (selectedServices != null)
            {
                foreach (var serviceId in selectedServices)
                {
                    _context.TrainerServices.Add(new TrainerService
                    {
                        TrainerId = trainer.Id,
                        ServiceTypeId = serviceId
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Antrenör başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Gyms = new SelectList(await _context.Gyms.ToListAsync(), "Id", "Name", trainer.GymId);
        ViewBag.Specialties = await _context.Specialties.ToListAsync();
        ViewBag.ServiceTypes = await _context.ServiceTypes.ToListAsync();
        return View(trainer);
    }

    // GET: Admin/Trainer/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var trainer = await _context.Trainers
            .Include(t => t.TrainerSpecialties)
            .Include(t => t.TrainerServices)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trainer == null)
            return NotFound();

        ViewBag.Gyms = new SelectList(await _context.Gyms.ToListAsync(), "Id", "Name", trainer.GymId);
        ViewBag.Specialties = await _context.Specialties.ToListAsync();
        ViewBag.ServiceTypes = await _context.ServiceTypes.ToListAsync();
        ViewBag.SelectedSpecialties = trainer.TrainerSpecialties.Select(ts => ts.SpecialtyId).ToList();
        ViewBag.SelectedServices = trainer.TrainerServices.Select(ts => ts.ServiceTypeId).ToList();

        return View(trainer);
    }

    // POST: Admin/Trainer/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Trainer trainer, int[] selectedSpecialties, int[] selectedServices)
    {
        if (id != trainer.Id)
            return NotFound();

        // Navigation property'leri ModelState'den kaldır
        ModelState.Remove("Gym");
        ModelState.Remove("TrainerSpecialties");
        ModelState.Remove("TrainerServices");
        ModelState.Remove("Availabilities");
        ModelState.Remove("Appointments");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(trainer);

                // eski iliskileri sil
                var oldSpecialties = _context.TrainerSpecialties.Where(ts => ts.TrainerId == id);
                _context.TrainerSpecialties.RemoveRange(oldSpecialties);

                var oldServices = _context.TrainerServices.Where(ts => ts.TrainerId == id);
                _context.TrainerServices.RemoveRange(oldServices);

                // yeni iliskileri ekle
                if (selectedSpecialties != null)
                {
                    foreach (var specialtyId in selectedSpecialties)
                    {
                        _context.TrainerSpecialties.Add(new TrainerSpecialty
                        {
                            TrainerId = trainer.Id,
                            SpecialtyId = specialtyId
                        });
                    }
                }

                if (selectedServices != null)
                {
                    foreach (var serviceId in selectedServices)
                    {
                        _context.TrainerServices.Add(new TrainerService
                        {
                            TrainerId = trainer.Id,
                            ServiceTypeId = serviceId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Antrenör başarıyla güncellendi!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrainerExists(trainer.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Gyms = new SelectList(await _context.Gyms.ToListAsync(), "Id", "Name", trainer.GymId);
        ViewBag.Specialties = await _context.Specialties.ToListAsync();
        ViewBag.ServiceTypes = await _context.ServiceTypes.ToListAsync();
        return View(trainer);
    }

    // GET: Admin/Trainer/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var trainer = await _context.Trainers
            .Include(t => t.Gym)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (trainer == null)
            return NotFound();

        return View(trainer);
    }

    // POST: Admin/Trainer/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer != null)
        {
            _context.Trainers.Remove(trainer);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Antrenör başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool TrainerExists(int id)
    {
        return _context.Trainers.Any(e => e.Id == id);
    }
}
