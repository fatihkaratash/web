using System.ComponentModel.DataAnnotations;

namespace FitMindAI.Models;

// antrenör müsaitlik takvimi - haftalık tekrarlı
public class TrainerAvailability
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Antrenör seçimi gereklidir")]
    [Display(Name = "Antrenör")]
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
    
    [Required(ErrorMessage = "Gün seçimi gereklidir")]
    [Range(0, 6, ErrorMessage = "Geçerli bir gün seçin")]
    [Display(Name = "Gün")]
    public int DayOfWeek { get; set; } // 0=Pazar ... 6=Cumartesi
    
    [Required(ErrorMessage = "Başlangıç saati gereklidir")]
    [Display(Name = "Başlangıç Saati")]
    public TimeOnly StartTime { get; set; }
    
    [Required(ErrorMessage = "Bitiş saati gereklidir")]
    [Display(Name = "Bitiş Saati")]
    public TimeOnly EndTime { get; set; }
}
