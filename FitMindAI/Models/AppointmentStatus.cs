namespace FitMindAI.Models;

public enum AppointmentStatus
{
    Pending = 0,    // onay bekliyor
    Approved = 1,   // onaylandı
    Rejected = 2,   // reddedildi
    Canceled = 3,   // iptal edildi
    Completed = 4   // tamamlandı
}
