namespace FitMindAI.ViewModels.Appointment
{
    // Confirm sayfası ve detay sayfaları için ViewModel
    public class AppointmentDetailsViewModel
    {
        // Randevu bilgileri
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Antrenör bilgileri
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string? TrainerBio { get; set; }
        
        // Salon bilgileri
        public int GymId { get; set; }
        public string GymName { get; set; } = string.Empty;
        public string GymAddress { get; set; } = string.Empty;
        
        // Hizmet bilgileri
        public int ServiceTypeId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceDescription { get; set; }
        public int ServiceDuration { get; set; }
    }
}
