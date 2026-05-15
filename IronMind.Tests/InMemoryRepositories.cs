using IronMind.Core.Interfaces;
using IronMind.Core.Models;

namespace IronMind.Tests;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> users = [];
    private int nextId = 1;

    public Task<bool> EmailExistsAsync(string email) =>
        Task.FromResult(users.Any(u => u.Email == email));

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(users.SingleOrDefault(u => u.Email == email));

    public Task<User?> GetByIdAsync(int userId) =>
        Task.FromResult(users.SingleOrDefault(u => u.Id == userId));

    public Task AddAsync(User user)
    {
        if (user.Id == 0) user.Id = nextId++;
        users.Add(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => Task.CompletedTask;
}

public class InMemoryExerciseRepository : IExerciseRepository
{
    private readonly List<WorkoutLog> logs = [];
    private int nextWorkoutId = 1;
    private int nextCardioId = 1;
    private int nextStrengthSetId = 1;

    public Task AddWorkoutLogAsync(WorkoutLog log)
    {
        if (log.Id == 0) log.Id = nextWorkoutId++;
        if (log.Cardio is not null)
        {
            if (log.Cardio.Id == 0) log.Cardio.Id = nextCardioId++;
            log.Cardio.WorkoutLogId = log.Id;
            log.Cardio.WorkoutLog = log;
        }

        foreach (var set in log.StrengthSets)
        {
            if (set.Id == 0) set.Id = nextStrengthSetId++;
            set.WorkoutLogId = log.Id;
            set.WorkoutLog = log;
        }

        logs.Add(log);
        return Task.CompletedTask;
    }

    public Task<WorkoutLog?> GetWorkoutLogAsync(int userId, int logId, bool includeDetails = true) =>
        Task.FromResult(logs.SingleOrDefault(w => w.Id == logId && w.UserId == userId));

    public Task<IReadOnlyList<WorkoutLog>> GetHistoryAsync(int userId, int pageSize, int page)
    {
        IReadOnlyList<WorkoutLog> result = logs
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.LoggedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(result);
    }

    public void DeleteWorkoutLog(WorkoutLog log) => logs.Remove(log);

    public void RemoveStrengthSets(IEnumerable<StrengthSet> sets)
    {
        foreach (var set in sets.ToList())
        {
            var parent = logs.SingleOrDefault(w => w.Id == set.WorkoutLogId);
            parent?.StrengthSets.Remove(set);
        }
    }

    public Task SaveChangesAsync() => Task.CompletedTask;
}
