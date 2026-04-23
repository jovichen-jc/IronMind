namespace IronMind.Core.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public float WeightKg { get; set; }
    public float HeightCm { get; set; }
    public UnitPreference Units { get; set; } = UnitPreference.Metric;
    public float DailyCalorieGoal { get; set; }
    public float DailyWaterGoalMl { get; set; }
    public string? DeviceToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MealLog> MealLogs { get; set; } = [];
    public ICollection<WorkoutLog> WorkoutLogs { get; set; } = [];
    public ICollection<WaterLog> WaterLogs { get; set; } = [];
    public ICollection<ReminderSchedule> ReminderSchedules { get; set; } = [];
}

public enum UnitPreference { Metric, Imperial }
