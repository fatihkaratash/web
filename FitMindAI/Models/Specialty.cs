namespace FitMindAI.Models;

// antrenör uzmanlık alanları
public class Specialty
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public ICollection<TrainerSpecialty> TrainerSpecialties { get; set; } = new List<TrainerSpecialty>();
}
