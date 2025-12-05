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
       
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API key bulunamadı. User Secrets yapılandırılmış mı?");
            }

        
            var prompt = BuildPrompt(heightCm, weightKg, age, gender, goal, experience, frequency, equipment, notes);

            
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
                    candidateCount = 1,
                    maxOutputTokens = 2000, 
                    temperature = 0.4,      
                    topP = 0.9,
                    topK = 20
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
                _logger.LogWarning("Gemini API'den geçersiz yapıda yanıt döndü. FinishReason: {Reason}", finishReason);
                
                // Token limiti durumunda kullanıcıya net mesaj
                if (finishReason == "MAX_TOKENS")
                {
                    return "⚠️ AI yanıtı kısa kesildi.\n\n" +
                           "Lütfen daha basit bilgiler girin veya 'Özel Notlar' kısmını kısa tutun.\n\n" +
                           "Örnek: 'Diz ağrım var' yerine sadece 'Diz sorunu' yazın.";
                }
                
                if (finishReason == "SAFETY")
                {
                    return "⚠️ AI güvenlik kuralları nedeniyle yanıt veremedi.\n\nLütfen farklı bilgiler girerek tekrar deneyin.";
                }
                
                return "❌ AI servisinden yanıt alınamadı.\n\nLütfen birkaç dakika sonra tekrar deneyin.";
            }

            var result = candidate.Content.Parts[0].Text;
            
            if (string.IsNullOrWhiteSpace(result))
            {
                _logger.LogWarning("Gemini API'den boş metin döndü");
                return "❌ AI yanıt üretemedi. Lütfen tekrar deneyin.";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API çağrısında hata oluştu");
            throw new ApplicationException("AI servisine bağlanırken bir sorun oluştu. Lütfen daha sonra tekrar deneyin.", ex);
        }
    }

    // Kullanıcı bilgilerine göre AI prompt hazırla
    private string BuildPrompt(int heightCm, int weightKg, int age, string gender, string goal, 
        string experience, string frequency, string equipment, string? notes)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("Sen profesyonel bir fitness antrenörüsün. Aşağıdaki üye için 1 haftalık antrenman programı hazırla.");
        sb.AppendLine("GÖREV: Önce üyenin hedefine dair 1-2 cümlelik motive edici kısa bir uzman yorumu yap, ardından programı listele.");
        sb.AppendLine();
        sb.AppendLine($"Üye Profili: {age} yaş, {gender}, {heightCm}cm, {weightKg}kg");
        sb.AppendLine($"Hedef: {goal}");
        sb.AppendLine($"Deneyim: {experience}");
        sb.AppendLine($"Sıklık: {frequency}");
        sb.AppendLine($"Ekipman: {equipment}");

        if (!string.IsNullOrWhiteSpace(notes))
        {
            sb.AppendLine($"Özel Durumlar/Notlar: {notes}");
        }

        sb.AppendLine();
        sb.AppendLine("İSTENEN FORMAT (Aynen bu yapıyı kullan):");
        sb.AppendLine("YORUM: [Buraya 1-2 cümlelik uzman görüşünü yaz]");
        sb.AppendLine();
        sb.AppendLine("--------------------------------------------------");
        sb.AppendLine("GÜN 1: [Odak Bölgesi]");
        sb.AppendLine("1. [Hareket Adı] - [Set]x[Tekrar]");
        sb.AppendLine("2. [Hareket Adı] - [Set]x[Tekrar]");
        sb.AppendLine("...");
        sb.AppendLine();
        sb.AppendLine("GÜN 2: [Odak Bölgesi]");
        sb.AppendLine("...");
        sb.AppendLine("--------------------------------------------------");
        sb.AppendLine("KURALLAR:");
        sb.AppendLine("1. Yorum kısmı maksimum 2 cümle olsun, çok uzatma.");
        sb.AppendLine("2. Hareket isimlerini Türkçe (veya yaygın İngilizce) kullan.");
        sb.AppendLine("3. Set ve tekrar sayılarını seviyeye uygun belirle.");
        sb.AppendLine("4. Yanıtın asla yarıda kesilmemeli, tam bir haftayı kapsasın.");

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
