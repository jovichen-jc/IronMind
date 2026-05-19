using System.Security.Claims;
using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;

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

        group.MapGet("/profile", async (IAuthService auth, ClaimsPrincipal user) =>
        {
            var profile = await auth.GetProfileAsync(GetUserId(user));
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization();

        group.MapPut("/profile", async (UpdateProfileRequest request, IAuthService auth, ClaimsPrincipal user) =>
        {
            var profile = await auth.UpdateProfileAsync(GetUserId(user), request);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization();
    }

    private static int GetUserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
