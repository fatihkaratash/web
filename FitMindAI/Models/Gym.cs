namespace FitMindAI.Models;

/// <summary>
/// Spor salonu bilgilerini temsil eder
/// </summary>
public class Gym
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Address { get; set; } = string.Empty;
    
    public string? Phone { get; set; }
    
    // Açılış saati - örn: 08:00
    public TimeOnly OpeningHour { get; set; }
    
    // Kapanış saati - örn: 22:00
    public TimeOnly ClosingHour { get; set; }
    
    // Navigation property: Bu salona bağlı antrenörler
    public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
}
