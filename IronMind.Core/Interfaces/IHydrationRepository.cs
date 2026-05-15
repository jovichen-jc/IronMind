using IronMind.Core.Models;

namespace IronMind.Core.Interfaces;

public interface IHydrationRepository
{
    Task AddWaterLogAsync(WaterLog log);
    Task<IReadOnlyList<WaterLog>> GetWaterLogsForDateAsync(int userId, DateOnly date);
    Task AddReminderScheduleAsync(ReminderSchedule reminder);
    Task<ReminderSchedule?> GetReminderScheduleAsync(int userId, int reminderId);
    Task SaveChangesAsync();
}
