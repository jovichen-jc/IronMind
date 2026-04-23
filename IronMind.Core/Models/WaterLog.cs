namespace IronMind.Core.Models;

public class WaterLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public float AmountMl { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}

public class ReminderSchedule
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int IntervalMinutes { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
