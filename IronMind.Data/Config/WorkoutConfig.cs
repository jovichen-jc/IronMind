using IronMind.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IronMind.Data.Config;

public class WorkoutLogConfig : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> builder)
    {
        builder.HasIndex(w => new { w.UserId, w.LoggedAt });
        builder.Property(w => w.Type).HasConversion<string>();
        builder.HasOne(w => w.Cardio).WithOne(c => c.WorkoutLog)
            .HasForeignKey<CardioDetails>(c => c.WorkoutLogId);
        builder.HasMany(w => w.StrengthSets).WithOne(s => s.WorkoutLog)
            .HasForeignKey(s => s.WorkoutLogId);
    }
}
