namespace DiabetesHelper.Models;

public class Meal
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime EffectiveDateUtc { get; set; } = DateTime.UtcNow;

    public string Description { get; set; } = string.Empty;

    public double CarbsGrams { get; set; }

    public string? Notes { get; set; }

    public string? PhotoPath { get; set; }
}
