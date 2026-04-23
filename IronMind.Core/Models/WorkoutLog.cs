namespace IronMind.Core.Models;

public class WorkoutLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public WorkoutType Type { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public CardioDetails? Cardio { get; set; }
    public ICollection<StrengthSet> StrengthSets { get; set; } = [];
}

public class CardioDetails
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public string ActivityType { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public float? DistanceKm { get; set; }
    public float? CaloriesBurned { get; set; }
}

public class StrengthSet
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public string ExerciseName { get; set; } = string.Empty;
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public float WeightKg { get; set; }
}

public enum WorkoutType { Cardio, Strength }
