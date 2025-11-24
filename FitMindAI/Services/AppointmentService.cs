using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;

namespace FitMindAI.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    private const int SlotIntervalMinutes = 30; // 30 dakika aralikla slot uretimi

    public AppointmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Trainer>> GetActiveTrainersAsync(int? gymId = null)
    {
        var query = _context.Trainers
            .Include(t => t.Gym)
            .Include(t => t.TrainerSpecialties)
                .ThenInclude(ts => ts.Specialty)
            .Where(t => t.IsActive);

        if (gymId.HasValue)
            query = query.Where(t => t.GymId == gymId.Value);

        return await query.OrderBy(t => t.FullName).ToListAsync();
    }

    public async Task<List<ServiceType>> GetServicesForTrainerAsync(int trainerId)
    {
        return await _context.TrainerServices
            .Where(ts => ts.TrainerId == trainerId)
            .Include(ts => ts.ServiceType)
            .Select(ts => ts.ServiceType)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<DateTime>> GetAvailableSlotsAsync(int trainerId, int serviceTypeId, DateOnly date)
    {
        // hizmet suresini al
        var service = await _context.ServiceTypes.FindAsync(serviceTypeId);
        if (service == null)
            return new List<DateTime>();

        var duration = service.DurationInMinutes;
        var dayOfWeek = (int)date.DayOfWeek;

        // o gun icin musaitlik kayitlarini al
        var availabilities = await _context.TrainerAvailabilities
            .Where(ta => ta.TrainerId == trainerId && ta.DayOfWeek == dayOfWeek)
            .OrderBy(ta => ta.StartTime)
            .ToListAsync();

        if (!availabilities.Any())
            return new List<DateTime>();

        // o gune ait tum randevulari al (iptal ve red edilmemis)
        var existingAppointments = await _context.Appointments
            .Where(a => a.TrainerId == trainerId &&
                        a.StartDateTime.Date == date.ToDateTime(TimeOnly.MinValue) &&
                        a.Status != AppointmentStatus.Canceled &&
                        a.Status != AppointmentStatus.Rejected)
            .ToListAsync();

        var slots = new List<DateTime>();

        // her musaitlik blogu icin slot uret
        foreach (var availability in availabilities)
        {
            var currentTime = availability.StartTime;
            var baseDate = date.ToDateTime(TimeOnly.MinValue);

            while (currentTime.AddMinutes(duration) <= availability.EndTime)
            {
                var slotDateTime = baseDate.Add(currentTime.ToTimeSpan());
                var slotEndDateTime = slotDateTime.AddMinutes(duration);

                // bu slotta cakisan randevu var mi kontrol et
                var hasOverlap = existingAppointments.Any(a =>
                    a.StartDateTime < slotEndDateTime &&
                    a.EndDateTime > slotDateTime);

                if (!hasOverlap)
                    slots.Add(slotDateTime);

                // 30dk ilerle
                currentTime = currentTime.AddMinutes(SlotIntervalMinutes);
            }
        }

        return slots;
    }

    public async Task<Appointment> CreateAppointmentAsync(string userId, int trainerId, int serviceTypeId, DateTime startDateTime)
    {
        // uyeyi bul
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException("Üye profili bulunamadı.");

        // hizmeti al
        var service = await _context.ServiceTypes.FindAsync(serviceTypeId);
        if (service == null)
            throw new InvalidOperationException("Hizmet bulunamadı.");

        var endDateTime = startDateTime.AddMinutes(service.DurationInMinutes);

        // 1. antrenor bu hizmeti veriyor mu?
        var trainerOffersService = await _context.TrainerServices
            .AnyAsync(ts => ts.TrainerId == trainerId && ts.ServiceTypeId == serviceTypeId);

        if (!trainerOffersService)
            throw new InvalidOperationException("Bu antrenör seçilen hizmeti vermiyor.");

        // 2. secilen saat antrenorun musaitlik saatlerinde mi?
        var dayOfWeek = (int)startDateTime.DayOfWeek;
        var timeOnly = TimeOnly.FromDateTime(startDateTime);
        var endTimeOnly = TimeOnly.FromDateTime(endDateTime);

        var isWithinAvailability = await _context.TrainerAvailabilities
            .AnyAsync(ta => ta.TrainerId == trainerId &&
                            ta.DayOfWeek == dayOfWeek &&
                            ta.StartTime <= timeOnly &&
                            ta.EndTime >= endTimeOnly);

        if (!isWithinAvailability)
            throw new InvalidOperationException("Seçilen saat antrenörün çalışma saatleri dışında.");

        // 3. o saatte cakisan baska randevu var mi?
        var hasOverlap = await _context.Appointments
            .AnyAsync(a => a.TrainerId == trainerId &&
                           (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Approved) &&
                           a.StartDateTime < endDateTime &&
                           a.EndDateTime > startDateTime);

        if (hasOverlap)
            throw new InvalidOperationException("Bu saatte antrenörün başka bir randevusu var.");

        // randevu olustur
        var appointment = new Appointment
        {
            MemberId = member.Id,
            TrainerId = trainerId,
            ServiceTypeId = serviceTypeId,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            TotalPrice = service.Price,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return appointment;
    }

    public async Task<List<Appointment>> GetMemberAppointmentsAsync(string userId)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null)
            return new List<Appointment>();

        return await _context.Appointments
            .Where(a => a.MemberId == member.Id)
            .Include(a => a.Trainer)
                .ThenInclude(t => t.Gym)
            .Include(a => a.ServiceType)
            .OrderByDescending(a => a.StartDateTime)
            .ToListAsync();
    }

    public async Task<bool> CancelAppointmentAsync(int appointmentId, string userId)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member == null)
            return false;

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.MemberId == member.Id);

        if (appointment == null)
            return false;

        // sadece gelecek tarihli ve henuz iptal edilmemis randevular iptal edilebilir
        if (appointment.StartDateTime <= DateTime.Now ||
            appointment.Status == AppointmentStatus.Canceled ||
            appointment.Status == AppointmentStatus.Rejected)
            return false;

        appointment.Status = AppointmentStatus.Canceled;
        await _context.SaveChangesAsync();

        return true;
    }
}
