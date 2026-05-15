using IronMind.Core.DTOs;

namespace IronMind.Core.Interfaces;

public interface IExerciseService
{
    Task<WorkoutLogDto> LogCardioAsync(int userId, LogCardioRequest request);
    Task<WorkoutLogDto> LogStrengthAsync(int userId, LogStrengthRequest request);
    Task<IEnumerable<WorkoutLogDto>> GetHistoryAsync(int userId, int pageSize = 20, int page = 1);
    Task<WorkoutLogDto> UpdateCardioAsync(int userId, int logId, UpdateCardioRequest request);
    Task<WorkoutLogDto> UpdateStrengthAsync(int userId, int logId, UpdateStrengthRequest request);
    Task DeleteWorkoutLogAsync(int userId, int logId);
}
