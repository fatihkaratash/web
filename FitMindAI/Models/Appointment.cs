namespace FitMindAI.Models;

public class Appointment
{
    public int Id { get; set; }
    
    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;
    
    public int TrainerId { get; set; }
    public Trainer Trainer { get; set; } = null!;
    
    public int ServiceTypeId { get; set; }
    public ServiceType ServiceType { get; set; } = null!;
    
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; } // otomatik hesaplanır
    public decimal TotalPrice { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
