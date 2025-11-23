namespace FitMindAI.Models;

// many-to-many bağlantı tablosu
public class TrainerSpecialty
{
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
    
    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
}
