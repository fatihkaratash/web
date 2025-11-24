using System.ComponentModel.DataAnnotations;

namespace FitMindAI.Models;

public class Trainer
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Antrenör adı gereklidir")]
    [MaxLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;
    
    [MaxLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir")]
    [Display(Name = "Biyografi")]
    public string? Bio { get; set; }
    
    [Required(ErrorMessage = "Salon seçimi gereklidir")]
    [Display(Name = "Salon")]
    public int GymId { get; set; }
    public Gym Gym { get; set; } = null!;
    
    [Display(Name = "Aktif mi?")]
    public bool IsActive { get; set; } = true; // aktif mi
    
    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    [Display(Name = "Avatar URL")]
    public string? AvatarUrl { get; set; }
    
    public ICollection<TrainerSpecialty> TrainerSpecialties { get; set; } = new List<TrainerSpecialty>();
    public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
    public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
