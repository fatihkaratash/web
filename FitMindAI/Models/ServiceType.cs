namespace FitMindAI.Models;

// hizmet türleri - Fitness, Yoga, Pilates
public class ServiceType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    
    public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
