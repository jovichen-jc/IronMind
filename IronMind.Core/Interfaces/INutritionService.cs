using IronMind.Core.DTOs;

namespace IronMind.Core.Interfaces;

public interface INutritionService
{
    Task<MealLogDto> LogMealAsync(int userId, LogMealRequest request);
    Task<DailySummaryDto> GetDailySummaryAsync(int userId, DateOnly date);
    Task<IEnumerable<MealLogDto>> GetMealLogsAsync(int userId, DateOnly date);
    Task<MealLogDto> UpdateMealLogAsync(int userId, int logId, UpdateMealRequest request);
    Task DeleteMealLogAsync(int userId, int logId);
    Task<IEnumerable<FoodSearchResult>> SearchFoodAsync(string query);
}
