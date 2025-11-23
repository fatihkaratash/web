namespace FitMindAI.Models;

public class Trainer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    
    public int GymId { get; set; }
    public Gym Gym { get; set; } = null!;
    
    public bool IsActive { get; set; } = true; // aktif mi
    public string? AvatarUrl { get; set; }
    
    public ICollection<TrainerSpecialty> TrainerSpecialties { get; set; } = new List<TrainerSpecialty>();
    public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
    public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
