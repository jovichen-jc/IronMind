using IronMind.Core.Models;

namespace IronMind.Core.Interfaces;

public interface INutritionRepository
{
    Task AddMealLogAsync(MealLog log);
    Task<MealLog?> GetMealLogAsync(int userId, int logId);
    Task<IReadOnlyList<MealLog>> GetMealLogsForDateAsync(int userId, DateOnly date);
    void DeleteMealLog(MealLog log);
    Task SaveChangesAsync();
}
