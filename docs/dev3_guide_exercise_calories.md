# Dev 3 — Exercise + Calorie Estimation Guide

**Your branch:** `dev3/exercise-calories`
**Files you own:** `IronMind.Services/ExerciseService.cs`
**No new files. No schema changes.**

---

## What Already Exists

- `POST /exercise/cardio` — logs a cardio session
- `POST /exercise/strength` — logs a strength session
- `GET /exercise/history` — paginated workout history
- `DELETE /exercise/{id}` — deletes a log entry
- `EstimateCalories` currently returns `durationMinutes * 8f` — a flat number regardless of activity

## What You Are Building

Replace the flat calorie estimate with a MET-based formula that accounts for activity type and user body weight.

---

## The Formula

```
Calories = MET × WeightKg × (DurationMinutes / 60)
```

**MET** (Metabolic Equivalent of Task) is a number that represents how hard an activity works the body relative to rest. Higher MET = more calories per minute.

| Activity (lowercase) | MET |
|----------------------|-----|
| running              | 9.8 |
| cycling              | 7.5 |
| swimming             | 8.0 |
| walking              | 3.5 |
| rowing               | 7.0 |
| elliptical           | 5.0 |
| *(anything else)*    | 6.0 |

Example: a 70 kg user runs for 30 minutes:
`9.8 × 70 × (30 / 60) = 343 calories`

Compare the old formula: `30 × 8 = 240 calories` — completely ignores the person's weight and what they were doing.

---

## Step 1 — Add the MET Lookup Table

Open `IronMind.Services/ExerciseService.cs`.

Add this private static dictionary inside the `ExerciseService` class:

```csharp
private static readonly Dictionary<string, float> MetValues = new(StringComparer.OrdinalIgnoreCase)
{
    ["running"]    = 9.8f,
    ["cycling"]    = 7.5f,
    ["swimming"]   = 8.0f,
    ["walking"]    = 3.5f,
    ["rowing"]     = 7.0f,
    ["elliptical"] = 5.0f,
};

private static float GetMet(string activityType) =>
    MetValues.TryGetValue(activityType.Trim(), out var met) ? met : 6.0f;
```

`StringComparer.OrdinalIgnoreCase` means "run", "Run", and "RUN" all match the same entry.

---

## Step 2 — Update EstimateCalories

Replace the existing `EstimateCalories` method:

```csharp
// before
private static float EstimateCalories(int durationMinutes) => durationMinutes * 8f;
```

With this:

```csharp
private static float EstimateCalories(string activityType, int durationMinutes, float weightKg)
{
    var met = GetMet(activityType);
    return MathF.Round(met * weightKg * (durationMinutes / 60f), 1);
}
```

---

## Step 3 — Fetch User Weight in LogCardioAsync

The formula needs the user's weight. `LogCardioAsync` currently doesn't look up the user. Fix that.

Replace the existing `LogCardioAsync` method with:

```csharp
public async Task<WorkoutLogDto> LogCardioAsync(int userId, LogCardioRequest request)
{
    var user = await db.Users.FindAsync(userId)
        ?? throw new KeyNotFoundException("User not found.");

    var log = new WorkoutLog
    {
        UserId = userId,
        Type = WorkoutType.Cardio,
        Notes = request.Notes,
        Cardio = new CardioDetails
        {
            ActivityType = request.ActivityType,
            DurationMinutes = request.DurationMinutes,
            DistanceKm = request.DistanceKm,
            CaloriesBurned = EstimateCalories(request.ActivityType, request.DurationMinutes, user.WeightKg)
        }
    };

    db.WorkoutLogs.Add(log);
    await db.SaveChangesAsync();
    return ToDto(log);
}
```

---

## Step 4 — Build and Test

```bash
cd IronMind.API
dotnet build
dotnet run
```

Open `http://localhost:5235/swagger`.

**Test flow:**
1. `POST /auth/register` — create a user with `"weightKg": 80`
2. Authorize with the token
3. `POST /exercise/cardio` — log a running session:
   ```json
   {
     "activityType": "running",
     "durationMinutes": 30
   }
   ```
4. Check `CaloriesBurned` in the response — should be `9.8 × 80 × 0.5 = 392`
5. Log a walking session (same duration, same user):
   ```json
   {
     "activityType": "walking",
     "durationMinutes": 30
   }
   ```
6. `CaloriesBurned` should be `3.5 × 80 × 0.5 = 140` — significantly less than running

**Also test:**
- `activityType: "RUNNING"` (uppercase) — should work, same result
- `activityType: "yoga"` (unknown activity) — should use the default MET of 6.0
- `GET /exercise/history` — confirm old logs still appear alongside new ones

---

## Done Looks Like

- Cardio logs show calorie estimates based on activity type and user weight
- Unknown activity types fall back to MET 6.0 instead of crashing
- `GET /exercise/history` still works and shows `CaloriesBurned` on each cardio entry
- Strength logs are unaffected (calorie estimation only applies to cardio)
- `dotnet build` has zero errors
