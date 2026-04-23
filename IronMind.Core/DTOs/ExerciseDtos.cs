using IronMind.Core.Models;

namespace IronMind.Core.DTOs;

public record LogCardioRequest(string ActivityType, int DurationMinutes,
    float? DistanceKm = null, string? Notes = null);

public record LogStrengthRequest(string ExerciseName, IEnumerable<SetEntry> Sets, string? Notes = null);

public record SetEntry(int SetNumber, int Reps, float WeightKg);

public record WorkoutLogDto(int Id, WorkoutType Type, DateTime LoggedAt, string? Notes,
    CardioDetailsDto? Cardio, IEnumerable<StrengthSetDto>? Sets);

public record CardioDetailsDto(string ActivityType, int DurationMinutes,
    float? DistanceKm, float? CaloriesBurned);

public record StrengthSetDto(string ExerciseName, int SetNumber, int Reps, float WeightKg);
