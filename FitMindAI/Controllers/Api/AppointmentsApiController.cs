using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Controllers.Api;

[Route("api/appointments")]
[ApiController]
[Authorize]
public class AppointmentsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AppointmentsApiController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Login olan üyenin randevuları
    [HttpGet("my")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { message = "Kullanıcı bulunamadı" });
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == user.Id);

        if (member == null)
        {
            return NotFound(new { message = "Üye kaydı bulunamadı" });
        }

        var appointments = await _context.Appointments
            .Include(a => a.Trainer)
                .ThenInclude(t => t.Gym)
            .Include(a => a.ServiceType)
            .Where(a => a.MemberId == member.Id)
            .OrderByDescending(a => a.StartDateTime)
            .Select(a => new
            {
                a.Id,
                Trainer = new
                {
                    a.Trainer.Id,
                    a.Trainer.FullName
                },
                Gym = a.Trainer.Gym.Name,
                Service = new
                {
                    a.ServiceType.Id,
                    a.ServiceType.Name,
                    a.ServiceType.Price
                },
                a.StartDateTime,
                a.EndDateTime,
                a.Status,
                a.TotalPrice,
                a.Notes
            })
            .ToListAsync();

        return Ok(new
        {
            memberName = member?.FullName ?? "Bilinmeyen",
            totalAppointments = appointments.Count,
            appointments
        });
    }

    // Randevu detayı (sadece kendi randevuna erişebilirsin)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == user.Id);

        if (member == null)
        {
            return NotFound(new { message = "Üye kaydı bulunamadı" });
        }

        var appointment = await _context.Appointments
            .Include(a => a.Trainer)
                .ThenInclude(t => t.Gym)
            .Include(a => a.ServiceType)
            .Where(a => a.Id == id && a.MemberId == member.Id)
            .Select(a => new
            {
                a.Id,
                Trainer = new
                {
                    a.Trainer.Id,
                    a.Trainer.FullName,
                    a.Trainer.Bio
                },
                Gym = a.Trainer.Gym != null ? new
                {
                    a.Trainer.Gym.Id,
                    Name = a.Trainer.Gym.Name,
                    Address = a.Trainer.Gym.Address,
                    Phone = a.Trainer.Gym.Phone
                } : null,
                Service = new
                {
                    a.ServiceType.Id,
                    a.ServiceType.Name,
                    a.ServiceType.DurationInMinutes,
                    a.ServiceType.Price,
                    a.ServiceType.Description
                },
                a.StartDateTime,
                a.EndDateTime,
                a.Status,
                a.TotalPrice,
                a.Notes,
                a.AdminNote,
                a.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return NotFound(new { message = "Randevu bulunamadı veya erişim yetkiniz yok" });
        }

        return Ok(appointment);
    }

    // Duruma göre randevuları filtrele (Pending, Approved, vb.)
    [HttpGet("by-status")]
    public async Task<IActionResult> GetByStatus([FromQuery] AppointmentStatus status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == user.Id);

        if (member == null)
        {
            return NotFound(new { message = "Üye kaydı bulunamadı" });
        }

        var appointments = await _context.Appointments
            .Include(a => a.Trainer)
            .Include(a => a.ServiceType)
            .Where(a => a.MemberId == member.Id && a.Status == status)
            .OrderByDescending(a => a.StartDateTime)
            .Select(a => new
            {
                a.Id,
                TrainerName = a.Trainer.FullName,
                ServiceName = a.ServiceType.Name,
                a.StartDateTime,
                a.EndDateTime,
                a.Status,
                a.TotalPrice
            })
            .ToListAsync();

        return Ok(new
        {
            status = status.ToString(),
            count = appointments.Count,
            appointments
        });
    }

    // Gelecek 7 gündeki randevularım
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == user.Id);

        if (member == null)
        {
            return NotFound(new { message = "Üye kaydı bulunamadı" });
        }

        var now = DateTime.UtcNow;
        var nextWeek = now.AddDays(7);

        var appointments = await _context.Appointments
            .Include(a => a.Trainer)
                .ThenInclude(t => t.Gym)
            .Include(a => a.ServiceType)
            .Where(a => a.MemberId == member.Id 
                     && a.StartDateTime >= now 
                     && a.StartDateTime <= nextWeek
                     && (a.Status == AppointmentStatus.Approved || a.Status == AppointmentStatus.Pending))
            .OrderBy(a => a.StartDateTime)
            .Select(a => new
            {
                a.Id,
                TrainerName = a.Trainer.FullName,
                GymName = a.Trainer.Gym.Name,
                ServiceName = a.ServiceType.Name,
                a.StartDateTime,
                a.EndDateTime,
                Status = a.Status.ToString(),
                DaysUntil = (int)((a.StartDateTime - now).TotalDays)
            })
            .ToListAsync();

        return Ok(new
        {
            message = "Gelecek 7 gün içindeki randevularınız",
            count = appointments.Count,
            appointments
        });
    }
}
