namespace FitMindAI.Models;

/// <summary>
/// Antrenör uzmanlık alanı - Kas Geliştirme, Kilo Verme, Kardiyo, vb.
/// </summary>
public class Specialty
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    // Navigation: Bu uzmanlığa sahip antrenörler
    public ICollection<TrainerSpecialty> TrainerSpecialties { get; set; } = new List<TrainerSpecialty>();
}
