# Dev 1 — Auth + Unit Conversion Guide

**Your branch:** `dev1/auth-units`
**Files you own:** `IronMind.Core/DTOs/AuthDtos.cs`, `IronMind.Core/Interfaces/IAuthService.cs`, `IronMind.Services/AuthService.cs`, `IronMind.API/Routes/AuthRoutes.cs`
**New file to create:** `IronMind.Core/UnitConverter.cs`

---

## What Already Exists

- `POST /auth/register` — creates a user and returns a JWT
- `POST /auth/login` — returns a JWT
- `User` model has: `WeightKg`, `HeightCm`, `Units` (Metric/Imperial), `DailyCalorieGoal`, `DailyWaterGoalMl`

## What You Are Building

1. `UnitConverter` — a static class used by all modules to convert metric values to imperial for API responses
2. `GET /auth/profile` — returns the logged-in user's profile in their preferred units
3. `PUT /auth/profile` — lets a user update their profile details

---

## Step 1 — Create UnitConverter

Create a new file: `IronMind.Core/UnitConverter.cs`

```csharp
namespace IronMind.Core;

public static class UnitConverter
{
    public static float KgToLbs(float kg) => MathF.Round(kg * 2.20462f, 1);
    public static float LbsToKg(float lbs) => MathF.Round(lbs / 2.20462f, 1);
    public static float KmToMiles(float km) => MathF.Round(km * 0.621371f, 2);
    public static float MilesToKm(float miles) => MathF.Round(miles / 0.621371f, 2);
    public static float MlToOz(float ml) => MathF.Round(ml * 0.033814f, 1);
    public static float OzToMl(float oz) => MathF.Round(oz / 0.033814f, 1);
    public static float CmToInches(float cm) => MathF.Round(cm * 0.393701f, 1);
}
```

This is the only file that does unit math. Every other module imports this.

---

## Step 2 — Add New DTOs

Open `IronMind.Core/DTOs/AuthDtos.cs` and add these records at the bottom:

```csharp
public record UserProfileDto(
    int Id,
    string Email,
    string Name,
    DateOnly DateOfBirth,
    float Weight,       // kg or lbs depending on Units
    float Height,       // cm or inches depending on Units
    string Units,       // "Metric" or "Imperial"
    float DailyCalorieGoal,
    float DailyWaterGoal,  // ml or oz depending on Units
    string? DeviceToken);

public record UpdateProfileRequest(
    string? Name = null,
    float? WeightKg = null,
    float? HeightCm = null,
    UnitPreference? Units = null,
    float? DailyCalorieGoal = null,
    float? DailyWaterGoalMl = null);
```

---

## Step 3 — Update the Interface

Open `IronMind.Core/Interfaces/IAuthService.cs` and add two new method signatures:

```csharp
Task<UserProfileDto?> GetProfileAsync(int userId);
Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileRequest request);
```

The full file should look like:

```csharp
using IronMind.Core.DTOs;

namespace IronMind.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<UserProfileDto?> GetProfileAsync(int userId);
    Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileRequest request);
}
```

---

## Step 4 — Implement in AuthService

Open `IronMind.Services/AuthService.cs` and add the two new methods. Add this `using` at the top:

```csharp
using IronMind.Core;
```

Then add these methods to the `AuthService` class:

```csharp
public async Task<UserProfileDto?> GetProfileAsync(int userId)
{
    var user = await db.Users.FindAsync(userId);
    return user is null ? null : ToProfileDto(user);
}

public async Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileRequest request)
{
    var user = await db.Users.FindAsync(userId);
    if (user is null) return null;

    if (request.Name is not null) user.Name = request.Name;
    if (request.WeightKg is not null) user.WeightKg = request.WeightKg.Value;
    if (request.HeightCm is not null) user.HeightCm = request.HeightCm.Value;
    if (request.Units is not null) user.Units = request.Units.Value;
    if (request.DailyCalorieGoal is not null) user.DailyCalorieGoal = request.DailyCalorieGoal.Value;
    if (request.DailyWaterGoalMl is not null) user.DailyWaterGoalMl = request.DailyWaterGoalMl.Value;

    await db.SaveChangesAsync();
    return ToProfileDto(user);
}

private static UserProfileDto ToProfileDto(User user)
{
    bool imperial = user.Units == UnitPreference.Imperial;
    return new UserProfileDto(
        user.Id,
        user.Email,
        user.Name,
        user.DateOfBirth,
        Weight: imperial ? UnitConverter.KgToLbs(user.WeightKg) : user.WeightKg,
        Height: imperial ? UnitConverter.CmToInches(user.HeightCm) : user.HeightCm,
        Units: user.Units.ToString(),
        user.DailyCalorieGoal,
        DailyWaterGoal: imperial ? UnitConverter.MlToOz(user.DailyWaterGoalMl) : user.DailyWaterGoalMl,
        user.DeviceToken);
}
```

---

## Step 5 — Add the Routes

Open `IronMind.API/Routes/AuthRoutes.cs` and add two new routes inside `MapAuthRoutes`. Add this `using` at the top:

```csharp
using System.Security.Claims;
```

Add these routes after the existing `/login` route:

```csharp
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
```

Then add the helper at the bottom of the class:

```csharp
private static int GetUserId(ClaimsPrincipal user) =>
    int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
```

---

## Step 6 — Build and Test

```bash
cd IronMind.API
dotnet build
dotnet run
```

Open `http://localhost:5235/swagger`.

**Test flow:**
1. `POST /auth/register` — register a user with `"units": 0` (Metric)
2. Copy the token from the response
3. Click **Authorize** at the top of Swagger, paste `Bearer <your_token>`
4. `GET /auth/profile` — confirm weight is in kg, water goal is in ml
5. `PUT /auth/profile` — change `"units": 1` (Imperial)
6. `GET /auth/profile` again — confirm weight is now in lbs, water goal is in oz

---

## Done Looks Like

- Profile endpoint returns data in the correct unit for both Metric and Imperial users
- Updating profile persists to the DB (re-run `GET /auth/profile` after restart to confirm)
- `UnitConverter.cs` is in `IronMind.Core` and builds cleanly — other devs will import it in Week 3
