using IronMind.Core.DTOs;

namespace IronMind.Core.Interfaces;

public interface IHydrationService
{
    Task<WaterLogDto> LogWaterAsync(int userId, LogWaterRequest request);
    Task<WaterSummaryDto> GetDailySummaryAsync(int userId, DateOnly date);
    Task<ReminderScheduleDto> SetReminderAsync(int userId, SetReminderRequest request);
    Task<bool> ToggleReminderAsync(int userId, int reminderId, bool isActive);
}
