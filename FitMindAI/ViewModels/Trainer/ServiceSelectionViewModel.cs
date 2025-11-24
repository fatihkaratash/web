namespace FitMindAI.ViewModels.Trainer
{
    // SelectService sayfası için ViewModel (antrenör + hizmetleri)
    public class ServiceSelectionViewModel
    {
        // Antrenör bilgileri
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string? TrainerBio { get; set; }
        public string GymName { get; set; } = string.Empty;
        
        // Hizmetler listesi
        public List<ServiceItemViewModel> Services { get; set; } = new();
    }
    
    public class ServiceItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationInMinutes { get; set; }
        public decimal Price { get; set; }
    }
}
