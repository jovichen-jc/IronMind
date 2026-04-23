namespace IronMind.Core.DTOs;

public record LogMealRequest(string FoodName, float Calories,
    float? ProteinG = null, float? CarbsG = null, float? FatG = null,
    string? OpenFoodFactsId = null);

public record MealLogDto(int Id, string FoodName, float Calories,
    float? ProteinG, float? CarbsG, float? FatG, DateTime LoggedAt);

public record DailySummaryDto(DateOnly Date, float TotalCalories, float CalorieGoal,
    float Remaining, IEnumerable<MealLogDto> Meals);

public record FoodSearchResult(string Id, string Name, float CaloriesPer100g,
    float? ProteinPer100g, float? CarbsPer100g, float? FatPer100g);
