using System.ComponentModel.DataAnnotations;

namespace FitMindAI.Models;

// antrenör uzmanlık alanları
public class Specialty
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Uzmanlık adı gereklidir")]
    [MaxLength(100, ErrorMessage = "Uzmanlık adı en fazla 100 karakter olabilir")]
    [Display(Name = "Uzmanlık Adı")]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<TrainerSpecialty> TrainerSpecialties { get; set; } = new List<TrainerSpecialty>();
}
