using SQLite;

namespace DiabetesHelper.Models;

public class Meal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public string Description { get; set; } = string.Empty;

    public double CarbsGrams { get; set; }

    public string? Notes { get; set; }
}
