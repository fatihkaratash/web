using FitMindAI.Models;

namespace FitMindAI.Services;

public interface IAppointmentService
{
    // aktif antrenorleri getir (opsiyonel gym filtreli)
    Task<List<Trainer>> GetActiveTrainersAsync(int? gymId = null);
    
    // antrenorun verdigi hizmetleri getir
    Task<List<ServiceType>> GetServicesForTrainerAsync(int trainerId);
    
    // belirli bir gun icin musait slotlari uret (30dk aralikla)
    Task<List<DateTime>> GetAvailableSlotsAsync(int trainerId, int serviceTypeId, DateOnly date);
    
    // randevu olustur (validationlar dahil)
    Task<Appointment> CreateAppointmentAsync(string userId, int trainerId, int serviceTypeId, DateTime startDateTime);
    
    // uyenin randevularini getir
    Task<List<Appointment>> GetMemberAppointmentsAsync(string userId);
    
    // randevu iptal et (sadece kendi randevusu + gelecek tarih)
    Task<bool> CancelAppointmentAsync(int appointmentId, string userId);
}
