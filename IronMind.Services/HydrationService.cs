using IronMind.Core.DTOs;
using IronMind.Core.Interfaces;
using IronMind.Core.Models;

namespace IronMind.Services;

public class HydrationService(IHydrationRepository hydration, IUserRepository users) : IHydrationService
{
    public async Task<WaterLogDto> LogWaterAsync(int userId, LogWaterRequest request)
    {
        var log = new WaterLog { UserId = userId, AmountMl = request.AmountMl };
        await hydration.AddWaterLogAsync(log);
        await hydration.SaveChangesAsync();
        return new WaterLogDto(log.Id, log.AmountMl, log.LoggedAt);
    }

    public async Task<WaterSummaryDto> GetDailySummaryAsync(int userId, DateOnly date)
    {
        var user = await users.GetByIdAsync(userId);
        var entries = await hydration.GetWaterLogsForDateAsync(userId, date);
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
        await hydration.AddReminderScheduleAsync(reminder);
        await hydration.SaveChangesAsync();
        return ToDto(reminder);
    }

    public async Task<bool> ToggleReminderAsync(int userId, int reminderId, bool isActive)
    {
        var reminder = await hydration.GetReminderScheduleAsync(userId, reminderId)
            ?? throw new KeyNotFoundException("Reminder not found.");
        reminder.IsActive = isActive;
        await hydration.SaveChangesAsync();
        return reminder.IsActive;
    }

    private static ReminderScheduleDto ToDto(ReminderSchedule r) =>
        new(r.Id, r.IntervalMinutes, r.StartTime, r.EndTime, r.IsActive);
}
