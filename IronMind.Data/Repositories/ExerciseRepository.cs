using IronMind.Core.Interfaces;
using IronMind.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Data.Repositories;

public class ExerciseRepository(AppDbContext db) : IExerciseRepository
{
    public async Task AddWorkoutLogAsync(WorkoutLog log) =>
        await db.WorkoutLogs.AddAsync(log);

    public Task<WorkoutLog?> GetWorkoutLogAsync(int userId, int logId, bool includeDetails = true)
    {
        IQueryable<WorkoutLog> query = db.WorkoutLogs;
        if (includeDetails)
        {
            query = query.Include(w => w.Cardio).Include(w => w.StrengthSets);
        }

        return query.SingleOrDefaultAsync(w => w.Id == logId && w.UserId == userId);
    }

    public async Task<IReadOnlyList<WorkoutLog>> GetHistoryAsync(int userId, int pageSize, int page) =>
        await db.WorkoutLogs
            .Include(w => w.Cardio)
            .Include(w => w.StrengthSets)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.LoggedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public void DeleteWorkoutLog(WorkoutLog log) => db.WorkoutLogs.Remove(log);

    public void RemoveStrengthSets(IEnumerable<StrengthSet> sets) => db.StrengthSets.RemoveRange(sets);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
