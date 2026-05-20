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

        group.MapGet("/profile", async (IAuthService auth, System.Security.Claims.ClaimsPrincipal user) =>
        {
            var profile = await auth.GetProfileAsync(user.GetUserId());
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization();

        group.MapPut("/profile", async (UpdateProfileRequest request, IAuthService auth, System.Security.Claims.ClaimsPrincipal user) =>
        {
            var profile = await auth.UpdateProfileAsync(user.GetUserId(), request);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization();

        group.MapPatch("/device-token", async (DeviceTokenRequest request, AppDbContext db, System.Security.Claims.ClaimsPrincipal user) =>
        {
            var u = await db.Users.FindAsync(user.GetUserId());
            if (u is null) return Results.NotFound();
            u.DeviceToken = request.Token;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
