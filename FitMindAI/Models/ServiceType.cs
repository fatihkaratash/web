namespace FitMindAI.Models;

/// <summary>
/// Hizmet türü - Fitness, Yoga, Pilates, vb.
/// </summary>
public class ServiceType
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    // Hizmet süresi (dakika cinsinden)
    public int DurationInMinutes { get; set; }
    
    // Hizmet ücreti (sabit fiyat)
    public decimal Price { get; set; }
    
    public string? Description { get; set; }
    
    // Navigation: Bu hizmeti veren antrenörler
    public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
}
