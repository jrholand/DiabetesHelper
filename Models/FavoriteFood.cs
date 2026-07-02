using SQLite;

namespace DiabetesHelper.Models;

public class FavoriteFood
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Trim().ToLowerInvariant() of Name, stored so lookups don't need a SQL LOWER()/TRIM() expression.
    public string NormalizedName { get; set; } = string.Empty;

    public double CarbsGrams { get; set; }

    public int? GlycemicIndex { get; set; }

    public int UseCount { get; set; }

    public DateTime LastUsedUtc { get; set; } = DateTime.UtcNow;
}
