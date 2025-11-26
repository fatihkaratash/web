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

    // Dashboard icin genel istatistikler
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            // Temel sayilar - LINQ Count() kullanimi
            var totalMembers = await _context.Members.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var totalGyms = await _context.Gyms.CountAsync();
            var totalTrainers = await _context.Trainers.CountAsync(t => t.IsActive);

            // Status bazli randevu sayilari - Where + Count LINQ
            var pendingCount = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
            var approvedCount = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Approved);
            var completedCount = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);
            var canceledCount = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Canceled);

            // En populer hizmet - Once veriyi cek, sonra memory'de grupla
            var appointments = await _context.Appointments
                .Include(a => a.ServiceType)
                .Include(a => a.Trainer)
                .ToListAsync();

            var mostPopularService = appointments
                .GroupBy(a => a.ServiceType?.Name ?? "Bilinmiyor")
                .Select(g => new { serviceName = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .FirstOrDefault();

            var topTrainer = appointments
                .GroupBy(a => a.Trainer?.FullName ?? "Bilinmiyor")
                .Select(g => new { trainerName = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .FirstOrDefault();

            return Ok(new
            {
                generatedAt = DateTime.UtcNow,
                linqUsed = "CountAsync, Where, Include, GroupBy, Select, OrderByDescending",
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
                    mostPopularService = mostPopularService?.serviceName ?? "Henuz randevu yok",
                    topTrainer = topTrainer?.trainerName ?? "Henuz randevu yok"
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // Salonlara gore antrenor ve randevu sayilari
    [HttpGet("gyms")]
    public async Task<IActionResult> GetGymStats()
    {
        var gyms = await _context.Gyms.ToListAsync();
        var trainers = await _context.Trainers.Where(t => t.IsActive).ToListAsync();
        var appointments = await _context.Appointments.Include(a => a.Trainer).ToListAsync();

        var gymStats = gyms.Select(g => new
        {
            id = g.Id,
            name = g.Name,
            trainers = trainers.Count(t => t.GymId == g.Id),
            appointments = appointments.Count(a => a.Trainer?.GymId == g.Id)
        })
        .OrderByDescending(x => x.appointments)
        .ToList();

        return Ok(gymStats);
    }

    // AI onerilerine ait istatistikler
    [HttpGet("ai-recommendations")]
    public async Task<IActionResult> GetAiStats()
    {
        var totalRecommendations = await _context.AiRecommendations.CountAsync();
        var todayRecommendations = await _context.AiRecommendations
            .CountAsync(r => r.CreatedAt.Date == DateTime.UtcNow.Date);

        // Hedef bazli dagilim - Once veri cek, sonra grupla
        var recommendations = await _context.AiRecommendations.ToListAsync();
        var goalDistribution = recommendations
            .GroupBy(r => r.Goal)
            .Select(g => new { goal = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToList();

        return Ok(new
        {
            total = totalRecommendations,
            today = todayRecommendations,
            byGoal = goalDistribution
        });
    }
}
