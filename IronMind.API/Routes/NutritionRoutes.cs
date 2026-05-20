using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;

namespace IronMind.API.Routes;

public static class NutritionRoutes
{
    public static void MapNutritionRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/nutrition").WithTags("Nutrition").RequireAuthorization();

        group.MapPost("/meals", async (LogMealRequest request, INutritionService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.LogMealAsync(user.GetUserId(), request)));

        group.MapGet("/meals", async (DateOnly? date, INutritionService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.GetMealLogsAsync(user.GetUserId(), date ?? DateOnly.FromDateTime(DateTime.UtcNow))));

        group.MapGet("/meals/summary", async (DateOnly? date, INutritionService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.GetDailySummaryAsync(user.GetUserId(), date ?? DateOnly.FromDateTime(DateTime.UtcNow))));

        group.MapDelete("/meals/{id:int}", async (int id, INutritionService svc, System.Security.Claims.ClaimsPrincipal user) =>
        {
            await svc.DeleteMealLogAsync(user.GetUserId(), id);
            return Results.NoContent();
        });

        group.MapGet("/food/search", async (string q, INutritionService svc) =>
            Results.Ok(await svc.SearchFoodAsync(q)));
    }
}
