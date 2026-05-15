using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Data.Repositories;

public class HydrationRepository(AppDbContext db) : IHydrationRepository
{
    public async Task AddWaterLogAsync(WaterLog log) =>
        await db.WaterLogs.AddAsync(log);

    public async Task<IReadOnlyList<WaterLog>> GetWaterLogsForDateAsync(int userId, DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await db.WaterLogs
            .Where(w => w.UserId == userId && w.LoggedAt >= start && w.LoggedAt <= end)
            .OrderBy(w => w.LoggedAt)
            .ToListAsync();
    }

    public async Task AddReminderScheduleAsync(ReminderSchedule reminder) =>
        await db.ReminderSchedules.AddAsync(reminder);

    public Task<ReminderSchedule?> GetReminderScheduleAsync(int userId, int reminderId) =>
        db.ReminderSchedules.SingleOrDefaultAsync(r => r.Id == reminderId && r.UserId == userId);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
