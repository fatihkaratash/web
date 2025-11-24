using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AppointmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public AppointmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Appointment
    public async Task<IActionResult> Index(int? status, int? trainerId)
    {
        // Tüm randevuları getir (Include ile ilişkili verileri yükle)
        var query = _context.Appointments
            .Include(a => a.Member)
            .Include(a => a.Trainer)
                .ThenInclude(t => t.Gym)
            .Include(a => a.ServiceType)
            .AsQueryable();

        // Status filtrelemesi
        if (status.HasValue)
        {
            query = query.Where(a => (int)a.Status == status.Value);
        }

        // Trainer filtrelemesi
        if (trainerId.HasValue)
        {
            query = query.Where(a => a.TrainerId == trainerId.Value);
        }

        // Tarihe göre sırala (en yeniler önce)
        var appointments = await query.OrderByDescending(a => a.StartDateTime).ToListAsync();

        // ViewBag ile filtreleme için gerekli verileri gönder
        ViewBag.Trainers = await _context.Trainers
            .Where(t => t.IsActive)
            .OrderBy(t => t.FullName)
            .ToListAsync();
        
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedTrainer = trainerId;

        return View(appointments);
    }

    // GET: Admin/Appointment/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Member)
            .Include(a => a.Trainer)
                .ThenInclude(t => t.Gym)
            .Include(a => a.ServiceType)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            return NotFound();
        }

        return View(appointment);
    }

    // POST: Admin/Appointment/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound();
        }

        // Sadece Pending durumundaki randevular onaylanabilir
        if (appointment.Status != AppointmentStatus.Pending)
        {
            TempData["Error"] = "Sadece onay bekleyen randevular onaylanabilir.";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = AppointmentStatus.Approved;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Randevu başarıyla onaylandı.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Appointment/Reject/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? adminNote)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound();
        }

        // Sadece Pending durumundaki randevular reddedilebilir
        if (appointment.Status != AppointmentStatus.Pending)
        {
            TempData["Error"] = "Sadece onay bekleyen randevular reddedilebilir.";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = AppointmentStatus.Canceled;
        appointment.AdminNote = adminNote; // Reddetme notu (opsiyonel)
        await _context.SaveChangesAsync();

        TempData["Success"] = "Randevu reddedildi.";
        return RedirectToAction(nameof(Index));
    }
}
