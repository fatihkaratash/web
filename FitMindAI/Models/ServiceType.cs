using System.ComponentModel.DataAnnotations;

namespace FitMindAI.Models;

// hizmet türleri - Fitness, Yoga, Pilates
public class ServiceType
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Hizmet adı gereklidir")]
    [MaxLength(100, ErrorMessage = "Hizmet adı en fazla 100 karakter olabilir")]
    [Display(Name = "Hizmet Adı")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Süre gereklidir")]
    [Range(15, 240, ErrorMessage = "Süre 15-240 dakika arasında olmalıdır")]
    [Display(Name = "Süre (Dakika)")]
    public int DurationInMinutes { get; set; }
    
    [Required(ErrorMessage = "Fiyat gereklidir")]
    [Range(0, 10000, ErrorMessage = "Fiyat 0-10000 arasında olmalıdır")]
    [Display(Name = "Fiyat (₺)")]
    public decimal Price { get; set; }
    
    [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
    
    public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
