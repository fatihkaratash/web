namespace FitMindAI.ViewModels;

public class AiRecommendationDetailViewModel
{
    public int Id { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string InputSummary { get; set; } = string.Empty;
    public string OutputText { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModelName { get; set; }
}
