using System.Security.Claims;
using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Data;

namespace IronMind.API.Routes;

public static class AuthRoutes
{
    public static void MapAuthRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService auth) =>
        {
            var result = await auth.RegisterAsync(request);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        group.MapPost("/login", async (LoginRequest request, IAuthService auth) =>
        {
            var result = await auth.LoginAsync(request);
            return result.Success ? Results.Ok(result) : Results.Unauthorized();
        });

        group.MapPatch("/device-token", async (DeviceTokenRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var u = await db.Users.FindAsync(userId);
            if (u is null) return Results.NotFound();
            u.DeviceToken = request.Token;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
