namespace FitMindAI.Models;

/// <summary>
/// Antrenör-Uzmanlık ilişkisi (Many-to-Many junction table)
/// Bir antrenörün birden fazla uzmanlığı olabilir
/// Bir uzmanlık alanında birden fazla antrenör olabilir
/// </summary>
public class TrainerSpecialty
{
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
    
    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
}
