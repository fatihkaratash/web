namespace FitMindAI.Models;

// PLACEHOLDER - Phase 2'de detaylı yapılacak
public class TrainerAvailability
{
    public int Id { get; set; }
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
}
