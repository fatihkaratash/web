namespace FitMindAI.Models;

// AI öneri kayıtları
public class AiRecommendation
{
    public int Id { get; set; }
    
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    
    // Input özet metni (JSON string: boy, kilo, yaş, hedef, vs.)
    public string InputSummary { get; set; } = string.Empty;
    
    // Gemini'den gelen full cevap
    public string OutputText { get; set; } = string.Empty;
    
    // Hedef (kolay filtreleme için)
    public string Goal { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // İleride lazım olabilir
    public string? ModelName { get; set; } // "gemini-1.5-flash"
}
