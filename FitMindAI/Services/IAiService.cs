namespace FitMindAI.Services;

public interface IAiService
{
    /// <summary>
    /// Gemini API'den kişiye özel antrenman programı önerisi al
    /// </summary>
    Task<string> GetWorkoutPlanAsync(
        int heightCm,
        int weightKg,
        int age,
        string gender,
        string goal,
        string experience,
        string frequency,
        string equipment,
        string? notes = null);
}
