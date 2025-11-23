namespace FitMindAI.Models;

// spor salonu bilgileri
public class Gym
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    public TimeOnly OpeningHour { get; set; } // açılış saati
    public TimeOnly ClosingHour { get; set; } // kapanış saati
    
    public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
}
