using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using IronMind.Services;

namespace IronMind.Tests;

public class ExerciseServiceTests
{
    [Fact]
    public async Task LogCardioAsync_UsesMetFormulaWithUserWeight()
    {
        var users = new InMemoryUserRepository();
        var exercise = new InMemoryExerciseRepository();
        var user = new User
        {
            Email = "runner@example.com",
            PasswordHash = "hash",
            Name = "Runner",
            DateOfBirth = new DateOnly(2000, 1, 1),
            WeightKg = 80,
            HeightCm = 180,
            DailyCalorieGoal = 2200,
            DailyWaterGoalMl = 2000
        };
        await users.AddAsync(user);
        await users.SaveChangesAsync();

        var service = new ExerciseService(exercise, users);
        var result = await service.LogCardioAsync(user.Id, new LogCardioRequest("running", 30));

        Assert.Equal(WorkoutType.Cardio, result.Type);
        Assert.NotNull(result.Cardio);
        Assert.Equal(392f, result.Cardio!.CaloriesBurned);
    }

    [Fact]
    public async Task LogCardioAsync_UsesDefaultMetForUnknownActivity()
    {
        var users = new InMemoryUserRepository();
        var exercise = new InMemoryExerciseRepository();
        var user = new User
        {
            Email = "yoga@example.com",
            PasswordHash = "hash",
            Name = "Yoga User",
            DateOfBirth = new DateOnly(2000, 1, 1),
            WeightKg = 70,
            HeightCm = 170,
            DailyCalorieGoal = 2000,
            DailyWaterGoalMl = 2000
        };
        await users.AddAsync(user);
        await users.SaveChangesAsync();

        var service = new ExerciseService(exercise, users);
        var result = await service.LogCardioAsync(user.Id, new LogCardioRequest("yoga", 30));

        Assert.Equal(210f, result.Cardio!.CaloriesBurned);
    }
}
