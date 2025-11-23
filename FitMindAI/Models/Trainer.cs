namespace FitMindAI.Models;

/// <summary>
/// Antrenör bilgileri
/// </summary>
public class Trainer
{
    public int Id { get; set; }
    
    public string FullName { get; set; } = string.Empty;
    
    public string? Bio { get; set; }
    
    // Hangi salona bağlı
    public int GymId { get; set; }
    public Gym Gym { get; set; } = null!;
    
    // Aktif mi? (Artık çalışmıyorsa false)
    public bool IsActive { get; set; } = true;
    
    public string? AvatarUrl { get; set; }
    
    // Navigation properties
    public ICollection<TrainerSpecialty> TrainerSpecialties { get; set; } = new List<TrainerSpecialty>();
    public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
    public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
