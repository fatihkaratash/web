using System.ComponentModel.DataAnnotations;

namespace FitMindAI.Models;

// spor salonu bilgileri
public class Gym
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Salon adı gereklidir")]
    [MaxLength(100, ErrorMessage = "Salon adı en fazla 100 karakter olabilir")]
    [Display(Name = "Salon Adı")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Adres gereklidir")]
    [MaxLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir")]
    [Display(Name = "Adres")]
    public string Address { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }
    
    [Display(Name = "Açılış Saati")]
    public TimeOnly OpeningHour { get; set; } // açılış saati
    
    [Display(Name = "Kapanış Saati")]
    public TimeOnly ClosingHour { get; set; } // kapanış saati
    
    public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
}
