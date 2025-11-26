using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;

namespace FitMindAI.Controllers.Api;

[Route("api/trainers")]
[ApiController]
public class TrainersApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TrainersApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Aktif antrenörlerin listesi (salon ve uzmanlık bilgileriyle)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var trainers = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerSpecialties)
                    .ThenInclude(ts => ts.Specialty)
                .Where(t => t.IsActive)
                .ToListAsync();

            var result = trainers.Select(t => new
            {
                t.Id,
                t.FullName,
                t.Bio,
                Gym = t.Gym?.Name ?? "Salon Belirtilmemis",
                Specialties = t.TrainerSpecialties != null 
                    ? t.TrainerSpecialties.Where(ts => ts.Specialty != null).Select(ts => ts.Specialty.Name).ToList() 
                    : new List<string>()
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Veri cekilirken hata olustu", detail = ex.Message });
        }
    }

    // Antrenör detayı - ID'ye göre bilgileri getir
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var trainer = await _context.Trainers
            .Include(t => t.Gym)
            .Include(t => t.TrainerSpecialties)
                .ThenInclude(ts => ts.Specialty)
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.FullName,
                t.Bio,
                t.IsActive,
                Gym = t.Gym != null ? new
                {
                    t.Gym.Id,
                    Name = t.Gym.Name,
                    Address = t.Gym.Address
                } : null,
                Specialties = t.TrainerSpecialties.Select(ts => new
                {
                    ts.Specialty.Id,
                    ts.Specialty.Name
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (trainer == null)
        {
            return NotFound(new { message = "Antrenör bulunamadı" });
        }

        return Ok(trainer);
    }

    // Seçilen tarihte çalışan antrenörler (günün müsaitlik saatlerine göre)
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] DateTime? date)
    {
        try
        {
            // Tarih kontrolu
            if (!date.HasValue)
            {
                return BadRequest(new { error = "Tarih parametresi gerekli", ornek = "?date=2025-11-27" });
            }

            // Gecmis tarih kontrolu
            if (date.Value.Date < DateTime.Today)
            {
                return BadRequest(new { error = "Gecmis tarih secilemez", secilen = date.Value.ToString("yyyy-MM-dd"), bugun = DateTime.Today.ToString("yyyy-MM-dd") });
            }

            var dayOfWeek = (int)date.Value.DayOfWeek;

            // Seçilen gün müsait olan antrenörler
            var availableTrainers = await _context.TrainerAvailabilities
                .Include(ta => ta.Trainer)
                    .ThenInclude(t => t.Gym)
                .Include(ta => ta.Trainer.TrainerSpecialties)
                    .ThenInclude(ts => ts.Specialty)
                .Where(ta => ta.DayOfWeek == dayOfWeek && ta.Trainer.IsActive)
                .Select(ta => ta.Trainer)
                .Distinct()
                .ToListAsync();

            var result = availableTrainers.Select(t => new
            {
                t.Id,
                t.FullName,
                t.Bio,
                Gym = t.Gym?.Name ?? "Belirtilmemis",
            Specialties = t.TrainerSpecialties != null 
                ? t.TrainerSpecialties.Where(ts => ts.Specialty != null).Select(ts => ts.Specialty.Name).ToList() 
                : new List<string>()
        });

            return Ok(new { tarih = date.Value.ToString("yyyy-MM-dd"), gun = date.Value.ToString("dddd"), antrenorSayisi = result.Count(), antrenorler = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Veri cekilirken hata olustu", detail = ex.Message });
        }
    }

    // Belirli bir antrenörün randevuları (tarih filtreleme ile)
    [HttpGet("{id}/appointments")]
    public async Task<IActionResult> GetAppointments(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var query = _context.Appointments
            .Include(a => a.Member)
            .Include(a => a.ServiceType)
            .Where(a => a.TrainerId == id);

        // Tarih filtresi
        if (startDate.HasValue)
        {
            query = query.Where(a => a.StartDateTime >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(a => a.EndDateTime <= endDate.Value);
        }

        var appointments = await query
            .OrderBy(a => a.StartDateTime)
            .Select(a => new
            {
                a.Id,
                MemberName = a.Member.FullName,
                Service = a.ServiceType.Name,
                a.StartDateTime,
                a.EndDateTime,
                a.Status,
                a.TotalPrice
            })
            .ToListAsync();

        return Ok(appointments);
    }
}
