namespace FitMindAI.ViewModels;

public class AiRecommendationListViewModel
{
    public int Id { get; set; }
    public string Goal { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gemini-2.5-flash";
    public DateTime CreatedAt { get; set; }
    public string OutputPreview { get; set; } = string.Empty; // İlk 150 karakter
}
