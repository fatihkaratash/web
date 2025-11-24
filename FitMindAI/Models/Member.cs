using Microsoft.AspNetCore.Identity;

namespace FitMindAI.Models;

public class Member
{
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty; // identity ile bağlantı
    public IdentityUser User { get; set; } = null!;
    
    public string FullName { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public int? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Goal { get; set; } // hedef
    
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();
}
