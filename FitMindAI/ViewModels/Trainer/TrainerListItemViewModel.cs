namespace FitMindAI.ViewModels.Trainer
{
    // SelectTrainer sayfasındaki antrenör kartları için ViewModel
    public class TrainerListItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public bool IsActive { get; set; }
        
        // Salon bilgileri
        public int GymId { get; set; }
        public string GymName { get; set; } = string.Empty;
        
        // Uzmanlıklar (virgülle ayrılmış)
        public string Specialties { get; set; } = string.Empty;
        
        // Hizmetler sayısı
        public int ServiceCount { get; set; }
    }
}
