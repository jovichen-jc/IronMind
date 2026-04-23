using IronMind.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace IronMind.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<CardioDetails> CardioDetails => Set<CardioDetails>();
    public DbSet<StrengthSet> StrengthSets => Set<StrengthSet>();
    public DbSet<WaterLog> WaterLogs => Set<WaterLog>();
    public DbSet<ReminderSchedule> ReminderSchedules => Set<ReminderSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
