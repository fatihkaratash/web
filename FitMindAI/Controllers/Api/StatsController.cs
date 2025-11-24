using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Dashboard istatistiklerini JSON formatında döner
        /// GET: api/stats
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            // Toplam sayılar
            var totalMembers = await _context.Members.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var totalGyms = await _context.Gyms.CountAsync();
            var totalTrainers = await _context.Trainers.CountAsync(t => t.IsActive);

            // Bugünkü randevular (geçici olarak 0 - UTC sorunu)
            var todayAppointments = 0;

            // Status bazlı randevu sayıları
            var pendingAppointments = await _context.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Pending);

            var approvedAppointments = await _context.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Approved);

            var completedAppointments = await _context.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Completed);

            var canceledAppointments = await _context.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Canceled);

            // En popüler hizmet (GroupBy + OrderBy LINQ)
            var mostPopularService = await _context.Appointments
                .GroupBy(a => a.ServiceType)
                .Select(g => new
                {
                    serviceName = g.Key!.Name,
                    appointmentCount = g.Count()
                })
                .OrderByDescending(x => x.appointmentCount)
                .FirstOrDefaultAsync();

            // En çok randevu alan antrenör (Include + ThenInclude + GroupBy)
            var topTrainer = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t!.Gym)
                .GroupBy(a => a.Trainer)
                .Select(g => new
                {
                    trainerName = g.Key!.FullName,
                    gymName = g.Key.Gym!.Name,
                    appointmentCount = g.Count()
                })
                .OrderByDescending(x => x.appointmentCount)
                .FirstOrDefaultAsync();

            // En aktif salon (subquery ile)
            var topGym = await _context.Gyms
                .Select(g => new
                {
                    gymName = g.Name,
                    trainerCount = _context.Trainers.Count(t => t.GymId == g.Id && t.IsActive),
                    appointmentCount = _context.Appointments.Count(a => a.Trainer.GymId == g.Id)
                })
                .OrderByDescending(x => x.appointmentCount)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                success = true,
                generatedAt = DateTime.Now,
                summary = new
                {
                    totalMembers,
                    totalAppointments,
                    totalGyms,
                    totalTrainers,
                    todayAppointments
                },
                appointmentsByStatus = new
                {
                    pending = pendingAppointments,
                    approved = approvedAppointments,
                    completed = completedAppointments,
                    canceled = canceledAppointments
                },
                insights = new
                {
                    mostPopularService,
                    topTrainer,
                    topGym
                }
            });
        }

        /// <summary>
        /// Salon bazlı istatistikler
        /// GET: api/stats/gyms
        /// </summary>
        [HttpGet("gyms")]
        public async Task<IActionResult> GetGymStats()
        {
            var gymStats = await _context.Gyms
                .Select(g => new
                {
                    id = g.Id,
                    name = g.Name,
                    address = g.Address,
                    activeTrainers = _context.Trainers.Count(t => t.GymId == g.Id && t.IsActive),
                    totalTrainers = _context.Trainers.Count(t => t.GymId == g.Id),
                    totalAppointments = _context.Appointments.Count(a => a.Trainer.GymId == g.Id),
                    pendingAppointments = _context.Appointments
                        .Count(a => a.Trainer.GymId == g.Id && a.Status == AppointmentStatus.Pending)
                })
                .OrderByDescending(x => x.totalAppointments)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                count = gymStats.Count,
                data = gymStats
            });
        }
    }
}
