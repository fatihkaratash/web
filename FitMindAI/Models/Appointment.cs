namespace FitMindAI.Models;

// PLACEHOLDER - Phase 2'de detaylı yapılacak
public class Appointment
{
    public int Id { get; set; }
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
}
