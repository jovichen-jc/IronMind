using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;

namespace IronMind.Services;

public class ExerciseService(IExerciseRepository exercise, IUserRepository users) : IExerciseService
{
    public async Task<WorkoutLogDto> LogCardioAsync(int userId, LogCardioRequest request)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

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
                CaloriesBurned = EstimateCalories(request.ActivityType, request.DurationMinutes, user.WeightKg)
            }
        };
        await exercise.AddWorkoutLogAsync(log);
        await exercise.SaveChangesAsync();
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
        await exercise.AddWorkoutLogAsync(log);
        await exercise.SaveChangesAsync();
        return ToDto(log);
    }

    public async Task<IEnumerable<WorkoutLogDto>> GetHistoryAsync(int userId, int pageSize = 20, int page = 1)
    {
        var logs = await exercise.GetHistoryAsync(userId, pageSize, page);
        return logs.Select(ToDto);
    }

    public async Task<WorkoutLogDto> UpdateCardioAsync(int userId, int logId, UpdateCardioRequest request)
    {
        var user = await users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var log = await exercise.GetWorkoutLogAsync(userId, logId)
            ?? throw new KeyNotFoundException("Workout log not found.");

        if (log.Type != WorkoutType.Cardio)
            throw new InvalidOperationException("Workout log is not a cardio entry.");

        log.Notes = request.Notes;
        log.Cardio ??= new CardioDetails { WorkoutLogId = log.Id };
        log.Cardio.ActivityType = request.ActivityType;
        log.Cardio.DurationMinutes = request.DurationMinutes;
        log.Cardio.DistanceKm = request.DistanceKm;
        log.Cardio.CaloriesBurned = EstimateCalories(request.ActivityType, request.DurationMinutes, user.WeightKg);

        await exercise.SaveChangesAsync();
        return ToDto(log);
    }

    public async Task<WorkoutLogDto> UpdateStrengthAsync(int userId, int logId, UpdateStrengthRequest request)
    {
        var log = await exercise.GetWorkoutLogAsync(userId, logId)
            ?? throw new KeyNotFoundException("Workout log not found.");

        if (log.Type != WorkoutType.Strength)
            throw new InvalidOperationException("Workout log is not a strength entry.");

        log.Notes = request.Notes;
        exercise.RemoveStrengthSets(log.StrengthSets);
        log.StrengthSets = request.Sets.Select(s => new StrengthSet
        {
            WorkoutLogId = log.Id,
            ExerciseName = request.ExerciseName,
            SetNumber = s.SetNumber,
            Reps = s.Reps,
            WeightKg = s.WeightKg
        }).ToList();

        await exercise.SaveChangesAsync();
        return ToDto(log);
    }

    public async Task DeleteWorkoutLogAsync(int userId, int logId)
    {
        var log = await exercise.GetWorkoutLogAsync(userId, logId, includeDetails: false)
            ?? throw new KeyNotFoundException("Workout log not found.");
        exercise.DeleteWorkoutLog(log);
        await exercise.SaveChangesAsync();
    }

    private static readonly Dictionary<string, float> MetValues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["running"] = 9.8f,
        ["cycling"] = 7.5f,
        ["swimming"] = 8.0f,
        ["walking"] = 3.5f,
        ["rowing"] = 7.0f,
        ["elliptical"] = 5.0f,
    };

    private static float GetMet(string activityType) =>
        MetValues.TryGetValue(activityType.Trim(), out var met) ? met : 6.0f;

    private static float EstimateCalories(string activityType, int durationMinutes, float weightKg)
    {
        var met = GetMet(activityType);
        return MathF.Round(met * weightKg * (durationMinutes / 60f), 1);
    }

    private static WorkoutLogDto ToDto(WorkoutLog w) => new(
        w.Id, w.Type, w.LoggedAt, w.Notes,
        w.Cardio is null ? null : new CardioDetailsDto(w.Cardio.ActivityType, w.Cardio.DurationMinutes, w.Cardio.DistanceKm, w.Cardio.CaloriesBurned),
        w.StrengthSets.Select(s => new StrengthSetDto(s.ExerciseName, s.SetNumber, s.Reps, s.WeightKg)));
}
