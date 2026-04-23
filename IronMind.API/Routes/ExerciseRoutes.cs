using System.Security.Claims;
using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;

namespace IronMind.API.Routes;

public static class ExerciseRoutes
{
    public static void MapExerciseRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/exercise").WithTags("Exercise").RequireAuthorization();

        group.MapPost("/cardio", async (LogCardioRequest request, IExerciseService svc, ClaimsPrincipal user) =>
            Results.Ok(await svc.LogCardioAsync(GetUserId(user), request)));

        group.MapPost("/strength", async (LogStrengthRequest request, IExerciseService svc, ClaimsPrincipal user) =>
            Results.Ok(await svc.LogStrengthAsync(GetUserId(user), request)));

        group.MapGet("/history", async (IExerciseService svc, ClaimsPrincipal user, int page = 1, int pageSize = 20) =>
            Results.Ok(await svc.GetHistoryAsync(GetUserId(user), pageSize, page)));

        group.MapDelete("/{id:int}", async (int id, IExerciseService svc, ClaimsPrincipal user) =>
        {
            await svc.DeleteWorkoutLogAsync(GetUserId(user), id);
            return Results.NoContent();
        });
    }

    private static int GetUserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
}
