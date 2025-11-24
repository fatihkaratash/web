using FitMindAI.Models;
using FitMindAI.ViewModels.Appointment;

namespace FitMindAI.Extensions
{
    public static class AppointmentExtensions
    {
        // Appointment entity'sini AppointmentListItemViewModel'e çevir
        public static AppointmentListItemViewModel ToListItemViewModel(this Appointment appointment)
        {
            return new AppointmentListItemViewModel
            {
                Id = appointment.Id,
                StartDateTime = appointment.StartDateTime,
                EndDateTime = appointment.EndDateTime,
                TotalPrice = appointment.TotalPrice,
                Status = appointment.Status,
                AdminNote = appointment.AdminNote,
                TrainerName = appointment.Trainer?.FullName ?? "Bilinmiyor",
                GymName = appointment.Trainer?.Gym?.Name ?? "Bilinmiyor",
                ServiceName = appointment.ServiceType?.Name ?? "Bilinmiyor",
                ServiceDuration = appointment.ServiceType?.DurationInMinutes ?? 0
            };
        }
        
        // Appointment entity'sini AppointmentDetailsViewModel'e çevir
        public static AppointmentDetailsViewModel ToDetailsViewModel(this Appointment appointment)
        {
            return new AppointmentDetailsViewModel
            {
                StartDateTime = appointment.StartDateTime,
                EndDateTime = appointment.EndDateTime,
                TotalPrice = appointment.TotalPrice,
                TrainerId = appointment.TrainerId,
                TrainerName = appointment.Trainer?.FullName ?? "Bilinmiyor",
                TrainerBio = appointment.Trainer?.Bio,
                GymId = appointment.Trainer?.GymId ?? 0,
                GymName = appointment.Trainer?.Gym?.Name ?? "Bilinmiyor",
                GymAddress = appointment.Trainer?.Gym?.Address ?? string.Empty,
                ServiceTypeId = appointment.ServiceTypeId,
                ServiceName = appointment.ServiceType?.Name ?? "Bilinmiyor",
                ServiceDescription = appointment.ServiceType?.Description,
                ServiceDuration = appointment.ServiceType?.DurationInMinutes ?? 0
            };
        }
        
        // IQueryable extension - birden fazla appointment'ı ViewModel'e çevir
        public static IEnumerable<AppointmentListItemViewModel> ToListItemViewModels(
            this IEnumerable<Appointment> appointments)
        {
            return appointments.Select(a => a.ToListItemViewModel());
        }
    }
}
