using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using IronMind.Data;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Services;

public class HydrationService(AppDbContext db) : IHydrationService
{
    public async Task<WaterLogDto> LogWaterAsync(int userId, LogWaterRequest request)
    {
        var log = new WaterLog { UserId = userId, AmountMl = request.AmountMl };
        db.WaterLogs.Add(log);
        await db.SaveChangesAsync();
        return new WaterLogDto(log.Id, log.AmountMl, log.LoggedAt);
    }

    public async Task<WaterSummaryDto> GetDailySummaryAsync(int userId, DateOnly date)
    {
        var user = await db.Users.FindAsync(userId);
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var entries = await db.WaterLogs
            .Where(w => w.UserId == userId && w.LoggedAt >= start && w.LoggedAt <= end)
            .OrderBy(w => w.LoggedAt)
            .ToListAsync();
        var total = entries.Sum(e => e.AmountMl);
        var dtos = entries.Select(e => new WaterLogDto(e.Id, e.AmountMl, e.LoggedAt));
        return new WaterSummaryDto(date, total, user!.DailyWaterGoalMl, user.DailyWaterGoalMl - total, dtos);
    }

    public async Task<ReminderScheduleDto> SetReminderAsync(int userId, SetReminderRequest request)
    {
        var reminder = new ReminderSchedule
        {
            UserId = userId,
            IntervalMinutes = request.IntervalMinutes,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };
        db.ReminderSchedules.Add(reminder);
        await db.SaveChangesAsync();
        return ToDto(reminder);
    }

    public async Task<bool> ToggleReminderAsync(int userId, int reminderId, bool isActive)
    {
        var reminder = await db.ReminderSchedules
            .SingleOrDefaultAsync(r => r.Id == reminderId && r.UserId == userId)
            ?? throw new KeyNotFoundException("Reminder not found.");
        reminder.IsActive = isActive;
        await db.SaveChangesAsync();
        return reminder.IsActive;
    }

    private static ReminderScheduleDto ToDto(ReminderSchedule r) =>
        new(r.Id, r.IntervalMinutes, r.StartTime, r.EndTime, r.IsActive);
}
