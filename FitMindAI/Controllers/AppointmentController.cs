using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FitMindAI.Services;
using FitMindAI.Data;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Extensions;
using FitMindAI.ViewModels.Trainer;

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

        // GET: Appointment/SelectGym - Ilk adim: salon sec
        public async Task<IActionResult> SelectGym()
        {
            var gyms = await _context.Gyms
                .OrderBy(g => g.Name)
                .ToListAsync();

            return View(gyms);
        }

        // GET: Appointment/SelectTrainer - Ikinci adim: antrenor sec
        public async Task<IActionResult> SelectTrainer(int? gymId)
        {
            // Eger gymId yoksa salon secim sayfasina yonlendir
            if (!gymId.HasValue)
            {
                return RedirectToAction(nameof(SelectGym));
            }

            // Secili salonu kontrol et
            var gym = await _context.Gyms.FindAsync(gymId.Value);
            if (gym == null)
            {
                return RedirectToAction(nameof(SelectGym));
            }

            ViewBag.SelectedGym = gym;

            // Antrenörleri getir ve ViewModel'e çevir
            var trainers = await _appointmentService.GetActiveTrainersAsync(gymId);
            var viewModels = trainers.ToListItemViewModels();
            
            return View(viewModels);
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

            // Antrenörün sunduğu servisleri getir
            var services = await _appointmentService.GetServicesForTrainerAsync(trainerId);
            
            if (!services.Any())
            {
                TempData["Error"] = "Bu antrenör için tanımlı servis bulunamadı.";
                return RedirectToAction(nameof(SelectTrainer));
            }

            // ViewModel oluştur
            var viewModel = trainer.ToServiceSelectionViewModel(services);

            return View(viewModel);
        }

        // POST: Appointment/CreateAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAppointment(int trainerId, int serviceTypeId, string appointmentDate, string appointmentTime)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (member == null)
            {
                TempData["Error"] = "Üye kaydınız bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            // Antrenör ve servisi kontrol et
            var trainer = await _context.Trainers.FindAsync(trainerId);
            var service = await _context.ServiceTypes.FindAsync(serviceTypeId);

            if (trainer == null || service == null)
            {
                TempData["Error"] = "Antrenör veya servis bulunamadı.";
                return RedirectToAction(nameof(SelectTrainer));
            }

            // Tarih ve saati parse et
            if (!DateTime.TryParse(appointmentTime, out var startDateTime))
            {
                TempData["Error"] = "Geçersiz tarih/saat formatı.";
                return RedirectToAction(nameof(SelectService), new { trainerId });
            }

            // Slot hala müsait mi kontrol et
            var date = DateOnly.FromDateTime(startDateTime);
            var availableSlots = await _appointmentService.GetAvailableSlotsAsync(trainerId, serviceTypeId, date);
            
            if (!availableSlots.Any(s => s == startDateTime))
            {
                TempData["Error"] = "Seçtiğiniz saat artık müsait değil. Lütfen başka bir saat seçin.";
                return RedirectToAction(nameof(SelectService), new { trainerId });
            }

            var endDateTime = startDateTime.AddMinutes(service.DurationInMinutes);

            var appointment = new Models.Appointment
            {
                MemberId = member.Id,
                TrainerId = trainerId,
                ServiceTypeId = serviceTypeId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                TotalPrice = service.Price,
                Status = Models.AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Randevunuz oluşturuldu! Tarih: {startDateTime:dd/MM/yyyy HH:mm}";
            return RedirectToAction(nameof(MyAppointments));
        }

        // POST: Appointment/QuickBook (ESKİ - SİL)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickBook(int trainerId, int serviceTypeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (member == null)
            {
                TempData["Error"] = "Üye kaydınız bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            // Antrenör ve servisi kontrol et
            var trainer = await _context.Trainers.FindAsync(trainerId);
            var service = await _context.ServiceTypes.FindAsync(serviceTypeId);

            if (trainer == null || service == null)
            {
                return NotFound();
            }

            // Randevu oluştur (otomatik tarih: yarından itibaren)
            var appointmentStart = DateTime.UtcNow.AddDays(1).Date.AddHours(10); // Yarın saat 10:00
            var appointmentEnd = appointmentStart.AddMinutes(service.DurationInMinutes);

            var appointment = new Models.Appointment
            {
                MemberId = member.Id,
                TrainerId = trainerId,
                ServiceTypeId = serviceTypeId,
                StartDateTime = appointmentStart,
                EndDateTime = appointmentEnd,
                TotalPrice = service.Price,
                Status = Models.AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Randevunuz oluşturuldu! Tarih: {appointmentStart:dd/MM/yyyy HH:mm} - Antrenör size dönüş yapacaktır.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // GET: Appointment/SelectDateTime?trainerId=X&serviceTypeId=Y (ESKİ - KALDIR)
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

            // ViewModel oluştur
            var viewModel = new ViewModels.Appointment.AppointmentDetailsViewModel
            {
                TrainerId = trainer.Id,
                TrainerName = trainer.FullName,
                TrainerBio = trainer.Bio,
                GymId = trainer.GymId,
                GymName = trainer.Gym?.Name ?? "Bilinmiyor",
                GymAddress = trainer.Gym?.Address ?? string.Empty,
                ServiceTypeId = service.Id,
                ServiceName = service.Name,
                ServiceDescription = service.Description,
                ServiceDuration = service.DurationInMinutes,
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddMinutes(service.DurationInMinutes),
                TotalPrice = service.Price
            };

            return View(viewModel);
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
            
            // Entity'leri ViewModel'e çevir
            var viewModels = appointments.ToListItemViewModels();
            
            return View(viewModels);
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
