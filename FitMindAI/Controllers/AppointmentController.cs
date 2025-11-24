using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FitMindAI.Services;
using FitMindAI.Data;
using Microsoft.EntityFrameworkCore;

namespace FitMindAI.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AppointmentController(
            IAppointmentService appointmentService,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
            _context = context;
        }

        // GET: Appointment/SelectTrainer
        public async Task<IActionResult> SelectTrainer(int? gymId)
        {
            // Gym filtresi için dropdown
            var gyms = await _context.Gyms
                .OrderBy(g => g.Name)
                .ToListAsync();
            ViewBag.Gyms = new SelectList(gyms, "Id", "Name", gymId);
            ViewBag.SelectedGymId = gymId;

            // Antrenörleri getir
            var trainers = await _appointmentService.GetActiveTrainersAsync(gymId);
            
            return View(trainers);
        }

        // GET: Appointment/SelectService?trainerId=X
        public async Task<IActionResult> SelectService(int trainerId)
        {
            // Antrenörü kontrol et
            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .FirstOrDefaultAsync(t => t.Id == trainerId);
            
            if (trainer == null)
            {
                return NotFound();
            }

            ViewBag.Trainer = trainer;

            // Antrenörün sunduğu servisleri getir
            var services = await _appointmentService.GetServicesForTrainerAsync(trainerId);
            
            if (!services.Any())
            {
                TempData["Error"] = "Bu antrenör için tanımlı servis bulunamadı.";
                return RedirectToAction(nameof(SelectTrainer));
            }

            return View(services);
        }

        // GET: Appointment/SelectDateTime?trainerId=X&serviceTypeId=Y
        // POST: Appointment/SelectDateTime (date seçildiğinde)
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> SelectDateTime(int trainerId, int serviceTypeId, DateTime? selectedDate, DateTime? selectedSlot)
        {
            // Antrenör ve servisi kontrol et
            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .FirstOrDefaultAsync(t => t.Id == trainerId);
            
            var service = await _context.ServiceTypes
                .FirstOrDefaultAsync(s => s.Id == serviceTypeId);

            if (trainer == null || service == null)
            {
                return NotFound();
            }

            ViewBag.Trainer = trainer;
            ViewBag.Service = service;
            ViewBag.TrainerId = trainerId;
            ViewBag.ServiceTypeId = serviceTypeId;

            // Eğer tarih seçildiyse, slotları getir
            if (selectedDate.HasValue)
            {
                var slots = await _appointmentService.GetAvailableSlotsAsync(trainerId, serviceTypeId, DateOnly.FromDateTime(selectedDate.Value));
                ViewBag.Slots = slots.Select(s => new SelectListItem
                {
                    Value = s.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Text = s.ToString("HH:mm")
                }).ToList();
                ViewBag.SelectedDate = selectedDate.Value.ToString("yyyy-MM-dd");
            }

            return View();
        }

        // GET: Appointment/Confirm?trainerId=X&serviceTypeId=Y&startDateTime=Z
        public async Task<IActionResult> Confirm(int trainerId, int serviceTypeId, DateTime startDateTime)
        {
            // Antrenör, servis ve slot kontrolü
            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .FirstOrDefaultAsync(t => t.Id == trainerId);
            
            var service = await _context.ServiceTypes
                .FirstOrDefaultAsync(s => s.Id == serviceTypeId);

            if (trainer == null || service == null)
            {
                return NotFound();
            }

            // Slot hala müsait mi kontrol et
            var date = DateOnly.FromDateTime(startDateTime);
            var availableSlots = await _appointmentService.GetAvailableSlotsAsync(trainerId, serviceTypeId, date);
            
            if (!availableSlots.Any(s => s == startDateTime))
            {
                TempData["Error"] = "Seçtiğiniz saat artık müsait değil. Lütfen başka bir saat seçin.";
                return RedirectToAction(nameof(SelectDateTime), new { trainerId, serviceTypeId });
            }

            ViewBag.Trainer = trainer;
            ViewBag.Service = service;
            ViewBag.StartDateTime = startDateTime;
            ViewBag.EndDateTime = startDateTime.AddMinutes(service.DurationInMinutes);

            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int trainerId, int serviceTypeId, DateTime startDateTime)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }

                var appointment = await _appointmentService.CreateAppointmentAsync(user.Id, trainerId, serviceTypeId, startDateTime);
                
                if (appointment != null)
                {
                    TempData["Success"] = "Randevunuz başarıyla oluşturuldu. Onay bekliyor.";
                    return RedirectToAction(nameof(MyAppointments));
                }
                else
                {
                    TempData["Error"] = "Randevu oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.";
                    return RedirectToAction(nameof(SelectTrainer));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(SelectTrainer));
            }
        }

        // GET: Appointment/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var appointments = await _appointmentService.GetMemberAppointmentsAsync(user.Id);
            
            return View(appointments);
        }

        // POST: Appointment/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }

                var result = await _appointmentService.CancelAppointmentAsync(id, user.Id);
                
                if (result)
                {
                    TempData["Success"] = "Randevunuz başarıyla iptal edildi.";
                }
                else
                {
                    TempData["Error"] = "Randevu iptal edilemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MyAppointments));
        }
    }
}
