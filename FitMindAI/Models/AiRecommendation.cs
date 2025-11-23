namespace FitMindAI.Models;

// AI öneri kayıtları
public class AiRecommendation
{
    public int Id { get; set; }
    
    public int? MemberId { get; set; } // null olabilir - guest için
    public Member? Member { get; set; }
    
    public string InputText { get; set; } = string.Empty; // JSON formatında
    public string ResultText { get; set; } = string.Empty;
    public string? Model { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
