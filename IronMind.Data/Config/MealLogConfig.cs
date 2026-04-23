using IronMind.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IronMind.Data.Config;

public class MealLogConfig : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.HasIndex(m => new { m.UserId, m.LoggedAt });
        builder.Property(m => m.FoodName).HasMaxLength(200).IsRequired();
    }
}
