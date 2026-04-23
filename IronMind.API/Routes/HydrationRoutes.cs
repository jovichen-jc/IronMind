using System.Security.Claims;
using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;

namespace IronMind.API.Routes;

public static class HydrationRoutes
{
    public static void MapHydrationRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/hydration").WithTags("Hydration").RequireAuthorization();

        group.MapPost("/water", async (LogWaterRequest request, IHydrationService svc, ClaimsPrincipal user) =>
            Results.Ok(await svc.LogWaterAsync(GetUserId(user), request)));

        group.MapGet("/water/summary", async (DateOnly? date, IHydrationService svc, ClaimsPrincipal user) =>
            Results.Ok(await svc.GetDailySummaryAsync(GetUserId(user), date ?? DateOnly.FromDateTime(DateTime.UtcNow))));

        group.MapPost("/reminders", async (SetReminderRequest request, IHydrationService svc, ClaimsPrincipal user) =>
            Results.Ok(await svc.SetReminderAsync(GetUserId(user), request)));

        group.MapPatch("/reminders/{id:int}", async (int id, bool active, IHydrationService svc, ClaimsPrincipal user) =>
            Results.Ok(await svc.ToggleReminderAsync(GetUserId(user), id, active)));
    }

    private static int GetUserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
}
