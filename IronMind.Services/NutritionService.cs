using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using IronMind.Data;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Services;

public class NutritionService(AppDbContext db) : INutritionService
{
    public async Task<MealLogDto> LogMealAsync(int userId, LogMealRequest request)
    {
        var log = new MealLog
        {
            UserId = userId,
            FoodName = request.FoodName,
            Calories = request.Calories,
            ProteinG = request.ProteinG,
            CarbsG = request.CarbsG,
            FatG = request.FatG,
            OpenFoodFactsId = request.OpenFoodFactsId
        };
        db.MealLogs.Add(log);
        await db.SaveChangesAsync();
        return ToDto(log);
    }

    public async Task<DailySummaryDto> GetDailySummaryAsync(int userId, DateOnly date)
    {
        var user = await db.Users.FindAsync(userId);
        var meals = await GetMealLogsAsync(userId, date);
        var total = meals.Sum(m => m.Calories);
        return new DailySummaryDto(date, total, user!.DailyCalorieGoal, user.DailyCalorieGoal - total, meals);
    }

    public async Task<IEnumerable<MealLogDto>> GetMealLogsAsync(int userId, DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var logs = await db.MealLogs
            .Where(m => m.UserId == userId && m.LoggedAt >= start && m.LoggedAt <= end)
            .OrderBy(m => m.LoggedAt)
            .ToListAsync();
        return logs.Select(ToDto);
    }

    public async Task DeleteMealLogAsync(int userId, int logId)
    {
        var log = await db.MealLogs.SingleOrDefaultAsync(m => m.Id == logId && m.UserId == userId)
            ?? throw new KeyNotFoundException("Meal log not found.");
        db.MealLogs.Remove(log);
        await db.SaveChangesAsync();
    }

    public Task<IEnumerable<FoodSearchResult>> SearchFoodAsync(string query)
    {
        // TODO (Dev 2 — Week 2): implement Open Food Facts API call
        throw new NotImplementedException("Open Food Facts integration — assigned to Dev 2.");
    }

    private static MealLogDto ToDto(MealLog m) =>
        new(m.Id, m.FoodName, m.Calories, m.ProteinG, m.CarbsG, m.FatG, m.LoggedAt);
}
