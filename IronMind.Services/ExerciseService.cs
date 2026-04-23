using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using IronMind.Data;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Services;

public class ExerciseService(AppDbContext db) : IExerciseService
{
    public async Task<WorkoutLogDto> LogCardioAsync(int userId, LogCardioRequest request)
    {
        var log = new WorkoutLog
        {
            UserId = userId,
            Type = WorkoutType.Cardio,
            Notes = request.Notes,
            Cardio = new CardioDetails
            {
                ActivityType = request.ActivityType,
                DurationMinutes = request.DurationMinutes,
                DistanceKm = request.DistanceKm,
                CaloriesBurned = EstimateCalories(request.DurationMinutes)
            }
        };
        db.WorkoutLogs.Add(log);
        await db.SaveChangesAsync();
        return ToDto(log);
    }

    public async Task<WorkoutLogDto> LogStrengthAsync(int userId, LogStrengthRequest request)
    {
        var log = new WorkoutLog
        {
            UserId = userId,
            Type = WorkoutType.Strength,
            Notes = request.Notes,
            StrengthSets = request.Sets.Select(s => new StrengthSet
            {
                ExerciseName = request.ExerciseName,
                SetNumber = s.SetNumber,
                Reps = s.Reps,
                WeightKg = s.WeightKg
            }).ToList()
        };
        db.WorkoutLogs.Add(log);
        await db.SaveChangesAsync();
        return ToDto(log);
    }

    public async Task<IEnumerable<WorkoutLogDto>> GetHistoryAsync(int userId, int pageSize = 20, int page = 1)
    {
        var logs = await db.WorkoutLogs
            .Include(w => w.Cardio)
            .Include(w => w.StrengthSets)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.LoggedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return logs.Select(ToDto);
    }

    public async Task DeleteWorkoutLogAsync(int userId, int logId)
    {
        var log = await db.WorkoutLogs.SingleOrDefaultAsync(w => w.Id == logId && w.UserId == userId)
            ?? throw new KeyNotFoundException("Workout log not found.");
        db.WorkoutLogs.Remove(log);
        await db.SaveChangesAsync();
    }

    // TODO (Dev 3 — Week 3): replace with MET-based formula per activity type
    private static float EstimateCalories(int durationMinutes) => durationMinutes * 8f;

    private static WorkoutLogDto ToDto(WorkoutLog w) => new(
        w.Id, w.Type, w.LoggedAt, w.Notes,
        w.Cardio is null ? null : new CardioDetailsDto(w.Cardio.ActivityType, w.Cardio.DurationMinutes, w.Cardio.DistanceKm, w.Cardio.CaloriesBurned),
        w.StrengthSets.Select(s => new StrengthSetDto(s.ExerciseName, s.SetNumber, s.Reps, s.WeightKg)));
}
