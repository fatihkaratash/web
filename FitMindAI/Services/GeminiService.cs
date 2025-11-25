using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitMindAI.Services;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetWorkoutPlanAsync(
        int heightCm,
        int weightKg,
        int age,
        string gender,
        string goal,
        string experience,
        string frequency,
        string equipment,
        string? notes = null)
    {
        try
        {
            // Gemini config
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API key bulunamadı. User Secrets yapılandırılmış mı?");
            }

            // Prompt oluştur
            var prompt = BuildPrompt(heightCm, weightKg, age, gender, goal, experience, frequency, equipment, notes);

            // Gemini request body with token limit for concise responses
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 1000,  // Artırıldı - thought tokens sorunu için
                    temperature = 0.7,
                    topP = 0.95
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // API endpoint - v1beta gerekli çünkü generateContent v1'de yok
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            _logger.LogInformation("Gemini API'ye istek gönderiliyor: {Model} (Bu işlem 20-40 saniye sürebilir)", model);
            
            var startTime = DateTime.UtcNow;
            var response = await _httpClient.PostAsync(endpoint, content);
            var duration = (DateTime.UtcNow - startTime).TotalSeconds;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API hatası ({Duration:F1}s): {StatusCode} - {Error}", 
                    duration, response.StatusCode, errorContent);
                throw new HttpRequestException($"Gemini API hatası: {response.StatusCode}");
            }

            _logger.LogInformation("Gemini API'den başarıyla yanıt alındı ({Duration:F1}s)", duration);

            var responseJson = await response.Content.ReadAsStringAsync();
            
            // Debug için response'u logla
            _logger.LogInformation("Gemini API response: {Response}", responseJson.Substring(0, Math.Min(500, responseJson.Length)));
            
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

            if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
            {
                _logger.LogWarning("Gemini API'den boş yanıt döndü");
                return "AI servisinden yanıt alınamadı. Lütfen daha sonra tekrar deneyin.";
            }

            var candidate = geminiResponse.Candidates[0];
            if (candidate?.Content?.Parts == null || candidate.Content.Parts.Length == 0)
            {
                var finishReason = candidate?.FinishReason ?? "UNKNOWN";
                _logger.LogWarning("Gemini API'den geçersiz yapıda yanıt döndü (Content veya Parts boş). FinishReason: {Reason}", finishReason);
                
                // Gemini 2.5-flash bazen MAX_TOKENS olduğunda thought tokens kullanıyor ama Parts boş geliyor
                if (finishReason == "MAX_TOKENS")
                {
                    return "AI modelinin yanıtı token limitine takıldı. Lütfen daha kısa bilgiler girerek tekrar deneyin veya birkaç dakika sonra tekrar deneyin.";
                }
                
                return "AI servisinden yanıt alınamadı. Lütfen daha sonra tekrar deneyin.";
            }

            var result = candidate.Content.Parts[0].Text;
            
            if (string.IsNullOrWhiteSpace(result))
            {
                _logger.LogWarning("Gemini API'den boş metin döndü");
                return "AI servisinden yanıt alınamadı. Lütfen daha sonra tekrar deneyin.";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API çağrısında hata oluştu");
            throw new ApplicationException("AI servisine bağlanırken bir sorun oluştu. Lütfen daha sonra tekrar deneyin.", ex);
        }
    }

    private string BuildPrompt(int heightCm, int weightKg, int age, string gender, string goal, 
        string experience, string frequency, string equipment, string? notes)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("Sen profesyonel bir fitness antrenörüsün. Kullanıcı bilgilerine göre KISA VE ÖZ bir haftalık antrenman programı hazırla.");
        sb.AppendLine();
        sb.AppendLine("KULLANICI BİLGİLERİ:");
        sb.AppendLine($"- Boy: {heightCm} cm");
        sb.AppendLine($"- Kilo: {weightKg} kg");
        sb.AppendLine($"- Yaş: {age}");
        sb.AppendLine($"- Cinsiyet: {gender}");
        sb.AppendLine($"- Hedef: {goal}");
        sb.AppendLine($"- Deneyim Seviyesi: {experience}");
        sb.AppendLine($"- Haftalık Antrenman Günü: {frequency}");
        sb.AppendLine($"- Ekipman: {equipment}");

        if (!string.IsNullOrWhiteSpace(notes))
        {
            sb.AppendLine($"- Özel Notlar: {notes}");
        }

        sb.AppendLine();
        sb.AppendLine("KURALLAR:");
        sb.AppendLine("- 1 haftalık (7 gün) program oluştur");
        sb.AppendLine("- Her antrenman günü için 4-5 egzersiz ver");
        sb.AppendLine("- Dinlenme günlerini belirt");
        sb.AppendLine("- Format: 'Egzersiz Adı – Set x Tekrar' (örnek: Bench Press – 4x8)");
        sb.AppendLine("- Uzun açıklamalar YAPMA, sadece liste ver");
        sb.AppendLine("- Markdown başlıkları KULLANMA");
        sb.AppendLine("- Toplam yanıt 400 kelimeyi geçmesin");
        sb.AppendLine("- Sade, kısa, liste halinde yaz");
        sb.AppendLine();
        sb.AppendLine("ÖRNEK FORMAT:");
        sb.AppendLine("Pazartesi (Göğüs-Triceps):");
        sb.AppendLine("- Bench Press – 4x8");
        sb.AppendLine("- Incline Dumbbell Press – 3x10");
        sb.AppendLine("- Cable Fly – 3x12");
        sb.AppendLine("- Triceps Dips – 3x10");
        sb.AppendLine();
        sb.AppendLine("Türkçe yaz ve sadece programı ver, gereksiz açıklama yapma.");

        return sb.ToString();
    }

    // Gemini API response modelleri
    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[] Candidates { get; set; } = Array.Empty<GeminiCandidate>();
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent Content { get; set; } = new();
        
        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
