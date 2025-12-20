using FitMindAI.Data;
using FitMindAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitMindAI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Toplam üye sayısı
            var totalMembers = await _context.Members.CountAsync();

            // Toplam randevu sayısı
            var totalAppointments = await _context.Appointments.CountAsync();

            // Bugünkü randevular - geçici olarak 0 (UTC sorunu)
            var todayAppointments = 0;

            // Bekleyen randevular (Pending status)
            var pendingAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Pending)
                .CountAsync();

            // Onaylanan randevular
            var approvedAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Approved)
                .CountAsync();

            // En popüler hizmet (en çok randevu alınan)
            var mostPopularService = await _context.Appointments
                .GroupBy(a => a.ServiceType)
                .Select(g => new
                {
                    ServiceType = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            // En çok randevu alan antrenör
            var topTrainer = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.Gym)
                .GroupBy(a => a.Trainer)
                .Select(g => new
                {
                    Trainer = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            // Son randevular (son 5 kayıt)
            var recentAppointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.ServiceType)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Salon kartları için veri
            var gyms = await _context.Gyms
                .Select(g => new
                {
                    Gym = g,
                    TrainerCount = _context.Trainers.Count(t => t.GymId == g.Id && t.IsActive),
                    TotalTrainers = _context.Trainers.Count(t => t.GymId == g.Id),
                    AppointmentCount = _context.Appointments.Count(a => a.Trainer.GymId == g.Id)
                })
                .ToListAsync();

            // ViewBag ile view'a gönder
            ViewBag.TotalMembers = totalMembers;
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.PendingAppointments = pendingAppointments;
            ViewBag.ApprovedAppointments = approvedAppointments;
            ViewBag.MostPopularService = mostPopularService;
            ViewBag.TopTrainer = topTrainer;
            ViewBag.RecentAppointments = recentAppointments;
            ViewBag.Gyms = gyms;

            return View();
        }

        // GET: Admin/Dashboard/ApiTest
        public IActionResult ApiTest()
        {
            return View();
        }
    }
}
