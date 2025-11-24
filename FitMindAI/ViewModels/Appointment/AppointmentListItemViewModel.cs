using FitMindAI.Models;

namespace FitMindAI.ViewModels.Appointment
{
    // MyAppointments listesinde her satır için ViewModel
    public class AppointmentListItemViewModel
    {
        public int Id { get; set; }
        
        // Randevu bilgileri
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal TotalPrice { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? AdminNote { get; set; }
        
        // Antrenör bilgileri
        public string TrainerName { get; set; } = string.Empty;
        
        // Salon bilgileri
        public string GymName { get; set; } = string.Empty;
        
        // Hizmet bilgileri
        public string ServiceName { get; set; } = string.Empty;
        public int ServiceDuration { get; set; }
        
        // UI için yardımcı özellikler
        public bool CanBeCanceled => 
            (Status == AppointmentStatus.Pending || Status == AppointmentStatus.Approved) 
            && StartDateTime > DateTime.Now;
        
        public string StatusBadgeClass => Status switch
        {
            AppointmentStatus.Pending => "warning",
            AppointmentStatus.Approved => "success",
            AppointmentStatus.Rejected => "danger",
            AppointmentStatus.Canceled => "secondary",
            AppointmentStatus.Completed => "info",
            _ => "secondary"
        };
        
        public string StatusText => Status switch
        {
            AppointmentStatus.Pending => "Onay Bekliyor",
            AppointmentStatus.Approved => "Onaylandı",
            AppointmentStatus.Rejected => "Reddedildi",
            AppointmentStatus.Canceled => "İptal Edildi",
            AppointmentStatus.Completed => "Tamamlandı",
            _ => "Bilinmiyor"
        };
    }
}
