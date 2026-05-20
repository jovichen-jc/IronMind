using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;

namespace IronMind.API.Routes;

public static class ExerciseRoutes
{
    public static void MapExerciseRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/exercise").WithTags("Exercise").RequireAuthorization();

        group.MapPost("/cardio", async (LogCardioRequest request, IExerciseService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.LogCardioAsync(user.GetUserId(), request)));

        group.MapPost("/strength", async (LogStrengthRequest request, IExerciseService svc, System.Security.Claims.ClaimsPrincipal user) =>
            Results.Ok(await svc.LogStrengthAsync(user.GetUserId(), request)));

        group.MapGet("/history", async (IExerciseService svc, System.Security.Claims.ClaimsPrincipal user, int page = 1, int pageSize = 20) =>
            Results.Ok(await svc.GetHistoryAsync(user.GetUserId(), pageSize, page)));

        group.MapDelete("/{id:int}", async (int id, IExerciseService svc, System.Security.Claims.ClaimsPrincipal user) =>
        {
            await svc.DeleteWorkoutLogAsync(user.GetUserId(), id);
            return Results.NoContent();
        });
    }
}
