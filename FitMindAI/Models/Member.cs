using Microsoft.AspNetCore.Identity;

namespace FitMindAI.Models;

/// <summary>
/// Üye profil bilgileri
/// </summary>
public class Member
{
    public int Id { get; set; }
    
    // Identity User ile ilişki (One-to-One)
    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;
    
    public string FullName { get; set; } = string.Empty;
    
    public DateTime BirthDate { get; set; }
    
    // Boy (cm cinsinden)
    public int HeightCm { get; set; }
    
    // Kilo (kg cinsinden, ondalıklı)
    public decimal WeightKg { get; set; }
    
    // Hedef - "Kilo Verme", "Kas Geliştirme", "Genel Fitness"
    public string Goal { get; set; } = string.Empty;
    
    public string? AvatarUrl { get; set; }
    
    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();
}
