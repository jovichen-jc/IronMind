namespace IronMind.Core.DTOs;

public record LogWaterRequest(float AmountMl);

public record WaterLogDto(int Id, float AmountMl, DateTime LoggedAt);

public record WaterSummaryDto(DateOnly Date, float TotalMl, float GoalMl,
    float RemainingMl, IEnumerable<WaterLogDto> Entries);

public record SetReminderRequest(int IntervalMinutes, TimeOnly StartTime, TimeOnly EndTime);

public record ReminderScheduleDto(int Id, int IntervalMinutes,
    TimeOnly StartTime, TimeOnly EndTime, bool IsActive);
