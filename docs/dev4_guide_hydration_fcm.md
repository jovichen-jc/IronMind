# Dev 4 — Hydration + FCM Push Notifications Guide

**Your branch:** `dev4/hydration-fcm`
**Files you own:** `IronMind.Services/HydrationService.cs`, `IronMind.API/Routes/HydrationRoutes.cs`, `IronMind.API/Routes/AuthRoutes.cs`, `IronMind.API/Program.cs`
**New files to create:** `IronMind.Core/Interfaces/INotificationService.cs`, `IronMind.Services/FcmNotificationService.cs`, `IronMind.Services/ReminderBackgroundService.cs`
**Schema change required:** adds `LastNotifiedAt` to `ReminderSchedules` table

---

## What Already Exists

- `POST /hydration/water` — log water intake
- `GET /hydration/water/summary` — daily water total vs goal
- `POST /hydration/reminders` — save a reminder schedule (interval, start time, end time)
- `PATCH /hydration/reminders/{id}` — toggle reminder on or off
- `User.DeviceToken` field exists in the DB but nothing saves to it

## What You Are Building

1. An endpoint to save the user's device token (`PATCH /auth/device-token`)
2. A Firebase Cloud Messaging (FCM) notification service
3. A background service that checks active reminder schedules and fires notifications at the right time

This is the most involved track. Read the whole guide before you start.

---

## Step 1 — Firebase Project Setup

You need a Firebase project to get push notification credentials.

1. Go to [console.firebase.google.com](https://console.firebase.google.com)
2. Create a new project (call it `IronMind`)
3. Go to **Project Settings → Service Accounts**
4. Click **Generate new private key** — this downloads a `.json` file
5. Rename it `firebase-service-account.json` and put it somewhere safe on your machine (NOT inside the repo folder)

Keep this file off GitHub. It contains credentials.

In your `appsettings.Development.json` (which is also gitignored), add:

```json
"Firebase": {
  "ServiceAccountPath": "/absolute/path/to/firebase-service-account.json"
}
```

---

## Step 2 — Add the Schema Migration

`ReminderSchedules` needs a `LastNotifiedAt` column so the background service knows when it last fired for each schedule.

Open `IronMind.Core/Models/WaterLog.cs` and add one property to the `ReminderSchedule` class:

```csharp
public DateTime? LastNotifiedAt { get; set; }
```

Then create the migration from the repo root:

```bash
dotnet ef migrations add AddLastNotifiedAt --project IronMind.Data --startup-project IronMind.API
dotnet ef database update --project IronMind.Data --startup-project IronMind.API
```

Push this migration to your branch. Tell the group you added it so nobody else creates a conflicting migration.

---

## Step 3 — Install the Firebase Admin SDK

Run this from the `IronMind.Services` project folder:

```bash
cd IronMind.Services
dotnet add package FirebaseAdmin
```

---

## Step 4 — Create the Notification Interface

Create `IronMind.Core/Interfaces/INotificationService.cs`:

```csharp
namespace IronMind.Core.Interfaces;

public interface INotificationService
{
    Task SendAsync(string deviceToken, string title, string body);
}
```

---

## Step 5 — Implement FcmNotificationService

Create `IronMind.Services/FcmNotificationService.cs`:

```csharp
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using IronMind.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IronMind.Services;

public class FcmNotificationService : INotificationService
{
    public FcmNotificationService(IConfiguration config)
    {
        if (FirebaseApp.DefaultInstance is not null) return;

        var path = config["Firebase:ServiceAccountPath"]
            ?? throw new InvalidOperationException("Firebase:ServiceAccountPath not set in config.");

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(path)
        });
    }

    public async Task SendAsync(string deviceToken, string title, string body)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification { Title = title, Body = body }
        };
        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }
}
```

---

## Step 6 — Implement the Background Service

Create `IronMind.Services/ReminderBackgroundService.cs`:

```csharp
using IronMind.Core.Interfaces;
using IronMind.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IronMind.Services;

public class ReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ReminderBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessRemindersAsync();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = TimeOnly.FromDateTime(DateTime.Now);
        var utcNow = DateTime.UtcNow;

        var schedules = await db.ReminderSchedules
            .Include(r => r.User)
            .Where(r => r.IsActive
                && r.StartTime <= now
                && r.EndTime >= now
                && r.User.DeviceToken != null)
            .ToListAsync();

        foreach (var schedule in schedules)
        {
            var minutesSinceLast = schedule.LastNotifiedAt.HasValue
                ? (utcNow - schedule.LastNotifiedAt.Value).TotalMinutes
                : double.MaxValue;

            if (minutesSinceLast < schedule.IntervalMinutes) continue;

            try
            {
                await notifier.SendAsync(
                    schedule.User.DeviceToken!,
                    "Hydration Reminder",
                    "Time to drink some water!");

                schedule.LastNotifiedAt = utcNow;
            }
            catch (Exception ex)
            {
                logger.LogWarning("Failed to send reminder {Id}: {Message}", schedule.Id, ex.Message);
            }
        }

        await db.SaveChangesAsync();
    }
}
```

The service:
- Wakes up every minute
- Finds all active reminders where the current time falls within the schedule's window
- Skips any that fired recently (within the interval)
- Sends an FCM notification and updates `LastNotifiedAt`
- Logs a warning on failure instead of crashing

---

## Step 7 — Add the Device Token Endpoint

Open `IronMind.API/Routes/AuthRoutes.cs`.

Add this route inside `MapAuthRoutes`, after the existing login route. You'll need a new DTO — add this record to `IronMind.Core/DTOs/AuthDtos.cs`:

```csharp
public record DeviceTokenRequest(string Token);
```

Then add to `AuthRoutes.cs`:

```csharp
group.MapPatch("/device-token", async (DeviceTokenRequest request, AppDbContext db, ClaimsPrincipal user) =>
{
    var userId = GetUserId(user);
    var u = await db.Users.FindAsync(userId);
    if (u is null) return Results.NotFound();
    u.DeviceToken = request.Token;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();
```

Add this `using` at the top of `AuthRoutes.cs`:

```csharp
using IronMind.Data;
```

---

## Step 8 — Register Services in Program.cs

Open `IronMind.API/Program.cs` and add these registrations before `var app = builder.Build()`:

```csharp
builder.Services.AddSingleton<INotificationService, FcmNotificationService>();
builder.Services.AddHostedService<ReminderBackgroundService>();
```

Add the required `using` statements:

```csharp
using IronMind.Services;
```

---

## Step 9 — Build and Test

```bash
cd IronMind.API
dotnet build
dotnet run
```

**Testing FCM without a real device:**

You can test that the background service is reading schedules correctly by checking the logs in the terminal — it will log warnings for any send failures. To test a real notification you need a device token from a mobile app (Firebase provides test tokens in the Firebase console under **Cloud Messaging → Send test message**).

**Test the device token endpoint:**

1. Register and login, get a JWT
2. Authorize in Swagger
3. `PATCH /auth/device-token` — send `{ "token": "test_device_token_abc123" }`
4. Should return `204 No Content`
5. Verify in the DB: `psql -d ironmind_dev -c "SELECT device_token FROM \"Users\" WHERE id = 1;"`

**Test the reminder setup:**

1. `POST /hydration/reminders`:
   ```json
   {
     "intervalMinutes": 120,
     "startTime": "08:00:00",
     "endTime": "22:00:00"
   }
   ```
2. Should return `200 OK` with the reminder details
3. Check the terminal — if the current time is between 08:00 and 22:00, you'll see the background service attempt to send a notification within 1 minute

---

## Done Looks Like

- `PATCH /auth/device-token` saves a token to the user record in the DB
- The background service starts with the API and appears in the startup logs
- Active reminder schedules within their time window trigger send attempts every interval
- Failed sends (bad token, network issue) are logged as warnings, not crashes
- `dotnet build` has zero errors
