using System.Security.Claims;

namespace IronMind.API;

internal static class ClaimsExtensions
{
    internal static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
