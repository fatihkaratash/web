using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Controllers.Api;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Dashboard için genel istatistikler
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        // Temel sayılar - LINQ Count() kullanımı
        var totalMembers = await _context.Members.CountAsync();
        var totalAppointments = await _context.Appointments.CountAsync();
        var totalGyms = await _context.Gyms.CountAsync();
        var totalTrainers = await _context.Trainers.CountAsync(t => t.IsActive);

        // Status bazlı randevu sayıları - Where + Count LINQ
        var pendingCount = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
        var approvedCount = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Approved);
        var completedCount = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);
        var canceledCount = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Canceled);

        // En popüler hizmet - GroupBy + OrderBy LINQ
        var mostPopularService = await _context.Appointments
            .GroupBy(a => a.ServiceType.Name)
            .Select(g => new
            {
                serviceName = g.Key,
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .FirstOrDefaultAsync();

        // En çok randevu alan antrenör - Include + GroupBy + OrderBy
        var topTrainer = await _context.Appointments
            .Include(a => a.Trainer)
            .GroupBy(a => a.Trainer.FullName)
            .Select(g => new
            {
                trainerName = g.Key,
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .FirstOrDefaultAsync();

        // Son 7 günlük randevu trendi - Where + GroupBy + Select
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var weeklyTrend = await _context.Appointments
            .Where(a => a.CreatedAt >= weekAgo)
            .GroupBy(a => a.CreatedAt.Date)
            .Select(g => new
            {
                date = g.Key.ToString("yyyy-MM-dd"),
                count = g.Count()
            })
            .OrderBy(x => x.date)
            .ToListAsync();

        return Ok(new
        {
            generatedAt = DateTime.UtcNow,
            summary = new
            {
                totalMembers,
                totalAppointments,
                totalGyms,
                totalTrainers
            },
            appointments = new
            {
                byStatus = new
                {
                    pending = pendingCount,
                    approved = approvedCount,
                    completed = completedCount,
                    canceled = canceledCount
                },
                mostPopularService = mostPopularService?.serviceName ?? "Henüz randevu yok",
                topTrainer = topTrainer?.trainerName ?? "Henüz randevu yok"
            },
            weeklyTrend
        });
    }

    // Salonlara göre antrenör ve randevu sayıları
    [HttpGet("gyms")]
    public async Task<IActionResult> GetGymStats()
    {
        var gymStats = await _context.Gyms
            .Select(g => new
            {
                id = g.Id,
                name = g.Name,
                trainers = _context.Trainers.Count(t => t.GymId == g.Id && t.IsActive),
                appointments = _context.Appointments.Count(a => a.Trainer.GymId == g.Id)
            })
            .OrderByDescending(x => x.appointments)
            .ToListAsync();

        return Ok(gymStats);
    }

    // AI önerilerine ait istatistikler (toplam, bugünkü, hedeflere göre dağılım)
    [HttpGet("ai-recommendations")]
    public async Task<IActionResult> GetAiStats()
    {
        var totalRecommendations = await _context.AiRecommendations.CountAsync();
        var todayRecommendations = await _context.AiRecommendations
            .CountAsync(r => r.CreatedAt.Date == DateTime.UtcNow.Date);

        // Hedef bazlı dağılım - GroupBy kullanımı
        var goalDistribution = await _context.AiRecommendations
            .GroupBy(r => r.Goal)
            .Select(g => new
            {
                goal = g.Key,
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .ToListAsync();

        return Ok(new
        {
            total = totalRecommendations,
            today = todayRecommendations,
            byGoal = goalDistribution
        });
    }
}
