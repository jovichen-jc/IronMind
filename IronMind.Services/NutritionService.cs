using System.Net.Http.Json;
using System.Text.Json.Serialization;
using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using IronMind.Data;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Services;

public class NutritionService(AppDbContext db, IHttpClientFactory httpClientFactory) : INutritionService
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

    public async Task<IEnumerable<FoodSearchResult>> SearchFoodAsync(string query)
    {
        var client = httpClientFactory.CreateClient("OpenFoodFacts");

        var url = $"cgi/search.pl?search_terms={Uri.EscapeDataString(query)}" +
                  "&search_simple=1&action=process&json=1&page_size=10" +
                  "&fields=code,product_name,nutriments";

        OffSearchResponse? response;
        try
        {
            response = await client.GetFromJsonAsync<OffSearchResponse>(url);
        }
        catch
        {
            return [];
        }

        if (response?.Products is null) return [];

        return response.Products
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
            .Select(p => new FoodSearchResult(
                Id: p.Code ?? string.Empty,
                Name: p.ProductName!,
                CaloriesPer100g: p.Nutriments?.EnergyKcal100g ?? 0,
                ProteinPer100g: p.Nutriments?.Proteins100g,
                CarbsPer100g: p.Nutriments?.Carbohydrates100g,
                FatPer100g: p.Nutriments?.Fat100g));
    }

    private static MealLogDto ToDto(MealLog m) =>
        new(m.Id, m.FoodName, m.Calories, m.ProteinG, m.CarbsG, m.FatG, m.LoggedAt);
}

file class OffSearchResponse
{
    [JsonPropertyName("products")]
    public List<OffProduct>? Products { get; set; }
}

file class OffProduct
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("nutriments")]
    public OffNutriments? Nutriments { get; set; }
}

file class OffNutriments
{
    [JsonPropertyName("energy-kcal_100g")]
    public float EnergyKcal100g { get; set; }

    [JsonPropertyName("proteins_100g")]
    public float? Proteins100g { get; set; }

    [JsonPropertyName("carbohydrates_100g")]
    public float? Carbohydrates100g { get; set; }

    [JsonPropertyName("fat_100g")]
    public float? Fat100g { get; set; }
}
