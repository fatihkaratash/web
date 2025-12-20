using FitMindAI.Models;
using FitMindAI.ViewModels.Trainer;

namespace FitMindAI.Extensions
{
    public static class TrainerExtensions
    {
        // Trainer entity'sini TrainerListItemViewModel'e çevir
        public static TrainerListItemViewModel ToListItemViewModel(this Trainer trainer)
        {
            // Uzmanlıkları virgülle ayır
            var specialties = trainer.TrainerSpecialties?
                .Select(ts => ts.Specialty?.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string?>();
            
            return new TrainerListItemViewModel
            {
                Id = trainer.Id,
                FullName = trainer.FullName,
                Bio = trainer.Bio,
                IsActive = trainer.IsActive,
                GymId = trainer.GymId,
                GymName = trainer.Gym?.Name ?? "Bilinmiyor",
                Specialties = specialties.Any() 
                    ? string.Join(", ", specialties) 
                    : "Belirtilmemiş",
                ServiceCount = trainer.TrainerServices?.Count ?? 0
            };
        }
        
        // Trainer + Services'i ServiceSelectionViewModel'e çevir
        public static ServiceSelectionViewModel ToServiceSelectionViewModel(
            this Trainer trainer, 
            IEnumerable<ServiceType> services)
        {
            return new ServiceSelectionViewModel
            {
                TrainerId = trainer.Id,
                TrainerName = trainer.FullName,
                TrainerBio = trainer.Bio,
                GymName = trainer.Gym?.Name ?? "Bilinmiyor",
                Services = services.Select(s => new ServiceItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    DurationInMinutes = s.DurationInMinutes,
                    Price = s.Price
                }).ToList()
            };
        }
        
        // IQueryable extension - birden fazla trainer'ı ViewModel'e çevir
        public static IEnumerable<TrainerListItemViewModel> ToListItemViewModels(
            this IEnumerable<Trainer> trainers)
        {
            return trainers.Select(t => t.ToListItemViewModel());
        }
    }
}
