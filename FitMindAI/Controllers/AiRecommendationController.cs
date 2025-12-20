using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.Models;
using FitMindAI.Services;
using FitMindAI.ViewModels;
using System.Text.Json;

namespace FitMindAI.Controllers;

[Authorize] // Sadece login kontrolü (Role kontrolü kaldırıldı - Member kaydı otomatik oluşturuluyor)
public class AiRecommendationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAiService _aiService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AiRecommendationController> _logger;

    public AiRecommendationController(
        ApplicationDbContext context,
        IAiService aiService,
        UserManager<IdentityUser> userManager,
        ILogger<AiRecommendationController> logger)
    {
        _context = context;
        _aiService = aiService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: AiRecommendation/Create
    public IActionResult Create()
    {
        var model = new AiRecommendationFormViewModel();
        return View(model);
    }

    // POST: AiRecommendation/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AiRecommendationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Kullanıcı bilgilerini al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (member == null)
            {
                ModelState.AddModelError("", "Üye kaydınız bulunamadı.");
                return View(model);
            }

            // Günlük limit kontrolü (günde en fazla 3 AI önerisi)
            var today = DateTime.UtcNow.Date;
            var todayCount = await _context.AiRecommendations
                .Where(r => r.MemberId == member.Id && r.CreatedAt.Date == today)
                .CountAsync();

            if (todayCount >= 3)
            {
                ModelState.AddModelError("", "Bugün için AI öneri limitine ulaştınız. Yarın tekrar deneyebilirsiniz.");
                return View(model);
            }

            // Gemini API'den öneri al
            _logger.LogInformation("Üye {MemberId} için AI önerisi isteniyor", member.Id);

            var aiResponse = await _aiService.GetWorkoutPlanAsync(
                model.HeightCm,
                model.WeightKg,
                model.Age,
                model.Gender,
                model.Goal,
                model.Experience,
                model.Frequency,
                model.Equipment,
                model.Notes);

            // Input summary oluştur (JSON)
            var inputSummary = JsonSerializer.Serialize(new
            {
                HeightCm = model.HeightCm,
                WeightKg = model.WeightKg,
                Age = model.Age,
                Gender = model.Gender,
                Goal = model.Goal,
                Experience = model.Experience,
                Frequency = model.Frequency,
                Equipment = model.Equipment,
                Notes = model.Notes
            });

            // Veritabanına kaydet
            var recommendation = new AiRecommendation
            {
                MemberId = member.Id,
                InputSummary = inputSummary,
                OutputText = aiResponse,
                Goal = model.Goal,
                ModelName = "gemini-1.5-flash",
                CreatedAt = DateTime.UtcNow
            };

            _context.AiRecommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("AI önerisi başarıyla kaydedildi: {Id}", recommendation.Id);

            // Sonucu model'e ekle ve aynı sayfada göster
            model.AiResponse = aiResponse;
            model.IsSuccess = true;

            TempData["SuccessMessage"] = "AI antrenman programınız başarıyla oluşturuldu!";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI önerisi oluşturulurken hata");
            ModelState.AddModelError("", "AI servisine bağlanırken bir sorun oluştu. Lütfen daha sonra tekrar deneyin.");
            return View(model);
        }
    }

    // GET: AiRecommendation/MyRecommendations
    public async Task<IActionResult> MyRecommendations()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == user.Id);

        if (member == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var recommendations = await _context.AiRecommendations
            .Where(r => r.MemberId == member.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new AiRecommendationListViewModel
            {
                Id = r.Id,
                Goal = r.Goal,
                ModelName = r.ModelName ?? "gemini-2.5-flash",
                CreatedAt = r.CreatedAt,
                OutputPreview = r.OutputText.Length > 150 
                    ? r.OutputText.Substring(0, 150) + "..." 
                    : r.OutputText
            })
            .ToListAsync();

        return View(recommendations);
    }

    // GET: AiRecommendation/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == user.Id);

        if (member == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var recommendation = await _context.AiRecommendations
            .FirstOrDefaultAsync(r => r.Id == id && r.MemberId == member.Id);

        if (recommendation == null)
        {
            return NotFound();
        }

        var model = new AiRecommendationDetailViewModel
        {
            Id = recommendation.Id,
            MemberName = member.FullName,
            InputSummary = recommendation.InputSummary,
            OutputText = recommendation.OutputText,
            Goal = recommendation.Goal,
            CreatedAt = recommendation.CreatedAt,
            ModelName = recommendation.ModelName
        };

        return View(model);
    }
}
