using System.ComponentModel.DataAnnotations;

namespace FitMindAI.ViewModels;

public class AiRecommendationFormViewModel
{
    [Required(ErrorMessage = "Boy gereklidir")]
    [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
    [Display(Name = "Boy (cm)")]
    public int HeightCm { get; set; }

    [Required(ErrorMessage = "Kilo gereklidir")]
    [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
    [Display(Name = "Kilo (kg)")]
    public int WeightKg { get; set; }

    [Required(ErrorMessage = "Yaş gereklidir")]
    [Range(14, 100, ErrorMessage = "Yaş 14-100 arasında olmalıdır")]
    [Display(Name = "Yaş")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Cinsiyet seçiniz")]
    [Display(Name = "Cinsiyet")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hedef seçiniz")]
    [Display(Name = "Hedef")]
    public string Goal { get; set; } = string.Empty;

    [Required(ErrorMessage = "Deneyim seviyesi seçiniz")]
    [Display(Name = "Deneyim Seviyesi")]
    public string Experience { get; set; } = string.Empty;

    [Required(ErrorMessage = "Antrenman sıklığı seçiniz")]
    [Display(Name = "Haftalık Antrenman Sıklığı")]
    public string Frequency { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ekipman durumu seçiniz")]
    [Display(Name = "Ekipman Durumu")]
    public string Equipment { get; set; } = string.Empty;

    [Display(Name = "Ek Notlar")]
    [StringLength(500, ErrorMessage = "Ek notlar en fazla 500 karakter olabilir")]
    public string? Notes { get; set; }

    public string? AiResponse { get; set; }
    public bool IsSuccess { get; set; }
}
