namespace FitMindAI.Models;

// PLACEHOLDER - Phase 2'de detaylı yapılacak
public class TrainerService
{
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
    
    public int ServiceTypeId { get; set; }
    public ServiceType ServiceType { get; set; } = null!;
}
