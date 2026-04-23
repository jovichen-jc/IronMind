using IronMind.Core.Models;

namespace IronMind.Core.DTOs;

public record RegisterRequest(string Email, string Password, string Name, DateOnly DateOfBirth,
    float WeightKg, float HeightCm, UnitPreference Units = UnitPreference.Metric,
    float DailyCalorieGoal = 2000, float DailyWaterGoalMl = 2000);

public record LoginRequest(string Email, string Password);

public record AuthResult(bool Success, string? Token, string? Error);
