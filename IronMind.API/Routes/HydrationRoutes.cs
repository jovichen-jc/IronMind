using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;

namespace IronMind.API.Routes;

public static class HydrationRoutes
{
    public static void MapHydrationRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/hydration").WithTags("Hydration").RequireAuthorization();

        group.MapPost("/water", async (LogWaterRequest request, IHydrationService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.LogWaterAsync(user.GetUserId(), request)));

        group.MapGet("/water/summary", async (DateOnly? date, IHydrationService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.GetDailySummaryAsync(user.GetUserId(), date ?? DateOnly.FromDateTime(DateTime.UtcNow))));

        group.MapPost("/reminders", async (SetReminderRequest request, IHydrationService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.SetReminderAsync(user.GetUserId(), request)));

        group.MapPatch("/reminders/{id:int}", async (int id, bool active, IHydrationService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.ToggleReminderAsync(user.GetUserId(), id, active)));
    }
}
