using System.Security.Claims;
using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;

namespace IronMind.API.Routes;

public static class NutritionRoutes
{
    public static void MapNutritionRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/nutrition").WithTags("Nutrition").RequireAuthorization();

        group.MapPost("/meals", async (LogMealRequest request, INutritionService svc, ClaimsPrincipal user) =>
            Results.Ok(await svc.LogMealAsync(GetUserId(user), request)));

        group.MapGet("/meals/summary", async (DateOnly? date, INutritionService svc, ClaimsPrincipal user) =>
            Results.Ok(await svc.GetDailySummaryAsync(GetUserId(user), date ?? DateOnly.FromDateTime(DateTime.UtcNow))));

        group.MapDelete("/meals/{id:int}", async (int id, INutritionService svc, ClaimsPrincipal user) =>
        {
            await svc.DeleteMealLogAsync(GetUserId(user), id);
            return Results.NoContent();
        });

        group.MapGet("/food/search", async (string q, INutritionService svc) =>
            Results.Ok(await svc.SearchFoodAsync(q)));
    }

    private static int GetUserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
}
