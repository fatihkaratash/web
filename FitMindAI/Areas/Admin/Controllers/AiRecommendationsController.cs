using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMindAI.Data;
using FitMindAI.ViewModels;

namespace FitMindAI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AiRecommendationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AiRecommendationsController> _logger;

    public AiRecommendationsController(
        ApplicationDbContext context,
        ILogger<AiRecommendationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Admin/AiRecommendations
    public async Task<IActionResult> Index()
    {
        var recommendations = await _context.AiRecommendations
            .Include(r => r.Member)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AiRecommendationListViewModel
            {
                Id = r.Id,
                Goal = r.Goal,
                CreatedAt = r.CreatedAt,
                OutputPreview = r.OutputText.Length > 150 
                    ? r.OutputText.Substring(0, 150) + "..." 
                    : r.OutputText
            })
            .Take(50) // Son 50 kayıt
            .ToListAsync();

        // İstatistikler
        var totalCount = await _context.AiRecommendations.CountAsync();
        var todayCount = await _context.AiRecommendations
            .Where(r => r.CreatedAt.Date == DateTime.UtcNow.Date)
            .CountAsync();
        
        var goalStats = await _context.AiRecommendations
            .GroupBy(r => r.Goal)
            .Select(g => new { Goal = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        ViewBag.TotalCount = totalCount;
        ViewBag.TodayCount = todayCount;
        ViewBag.GoalStats = goalStats;

        return View(recommendations);
    }

    // GET: Admin/AiRecommendations/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var recommendation = await _context.AiRecommendations
            .Include(r => r.Member)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recommendation == null)
        {
            return NotFound();
        }

        var model = new AiRecommendationDetailViewModel
        {
            Id = recommendation.Id,
            MemberName = recommendation.Member.FullName,
            InputSummary = recommendation.InputSummary,
            OutputText = recommendation.OutputText,
            Goal = recommendation.Goal,
            CreatedAt = recommendation.CreatedAt,
            ModelName = recommendation.ModelName
        };

        return View(model);
    }
}
