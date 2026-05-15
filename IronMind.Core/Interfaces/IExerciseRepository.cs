using IronMind.Core.Models;

namespace IronMind.Core.Interfaces;

public interface IExerciseRepository
{
    Task AddWorkoutLogAsync(WorkoutLog log);
    Task<WorkoutLog?> GetWorkoutLogAsync(int userId, int logId, bool includeDetails = true);
    Task<IReadOnlyList<WorkoutLog>> GetHistoryAsync(int userId, int pageSize, int page);
    void DeleteWorkoutLog(WorkoutLog log);
    void RemoveStrengthSets(IEnumerable<StrengthSet> sets);
    Task SaveChangesAsync();
}
