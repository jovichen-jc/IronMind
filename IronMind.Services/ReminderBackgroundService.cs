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
