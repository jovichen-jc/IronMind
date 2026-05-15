using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Data.Repositories;

public class NutritionRepository(AppDbContext db) : INutritionRepository
{
    public async Task AddMealLogAsync(MealLog log) =>
        await db.MealLogs.AddAsync(log);

    public Task<MealLog?> GetMealLogAsync(int userId, int logId) =>
        db.MealLogs.SingleOrDefaultAsync(m => m.Id == logId && m.UserId == userId);

    public async Task<IReadOnlyList<MealLog>> GetMealLogsForDateAsync(int userId, DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await db.MealLogs
            .Where(m => m.UserId == userId && m.LoggedAt >= start && m.LoggedAt <= end)
            .OrderBy(m => m.LoggedAt)
            .ToListAsync();
    }

    public void DeleteMealLog(MealLog log) => db.MealLogs.Remove(log);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
