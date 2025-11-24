using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitMindAI.Services;
using FitMindAI.Extensions;

namespace FitMindAI.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TrainersController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public TrainersController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        /// <summary>
        /// Aktif antrenörleri listeler (opsiyonel gym filtresi ile)
        /// GET: api/trainers?gymId=1
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTrainers([FromQuery] int? gymId)
        {
            var trainers = await _appointmentService.GetActiveTrainersAsync(gymId);
            var viewModels = trainers.ToListItemViewModels();
            
            return Ok(new
            {
                success = true,
                count = viewModels.Count(),
                data = viewModels
            });
        }

        /// <summary>
        /// Belirli bir antrenörün detaylarını getirir
        /// GET: api/trainers/5
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrainer(int id)
        {
            var trainers = await _appointmentService.GetActiveTrainersAsync(null);
            var trainer = trainers.FirstOrDefault(t => t.Id == id);
            
            if (trainer == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Antrenör bulunamadı"
                });
            }

            var viewModel = trainer.ToListItemViewModel();
            
            return Ok(new
            {
                success = true,
                data = viewModel
            });
        }

        /// <summary>
        /// Antrenörün belirli bir tarih ve hizmet için müsait saatlerini döner
        /// GET: api/trainers/5/available-slots?serviceTypeId=3&date=2025-11-25
        /// </summary>
        [HttpGet("{id}/available-slots")]
        public async Task<IActionResult> GetAvailableSlots(
            int id,
            [FromQuery] int serviceTypeId,
            [FromQuery] string date)
        {
            if (string.IsNullOrEmpty(date) || !DateOnly.TryParse(date, out var parsedDate))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Geçerli bir tarih belirtiniz (format: YYYY-MM-DD)"
                });
            }

            if (serviceTypeId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Geçerli bir serviceTypeId belirtiniz"
                });
            }

            var slots = await _appointmentService.GetAvailableSlotsAsync(id, serviceTypeId, parsedDate);
            
            if (!slots.Any())
            {
                return Ok(new
                {
                    success = true,
                    message = "Bu tarih için müsait slot bulunamadı",
                    count = 0,
                    data = Array.Empty<object>()
                });
            }

            var slotData = slots.Select(s => new
            {
                dateTime = s,
                date = s.ToString("yyyy-MM-dd"),
                time = s.ToString("HH:mm"),
                displayText = s.ToString("dd MMMM yyyy HH:mm")
            });

            return Ok(new
            {
                success = true,
                count = slotData.Count(),
                trainerId = id,
                serviceTypeId,
                requestedDate = date,
                data = slotData
            });
        }
    }
}
