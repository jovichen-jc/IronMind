namespace IronMind.Core.Models;

public class MealLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string FoodName { get; set; } = string.Empty;
    public float Calories { get; set; }
    public float? ProteinG { get; set; }
    public float? CarbsG { get; set; }
    public float? FatG { get; set; }
    public string? OpenFoodFactsId { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
