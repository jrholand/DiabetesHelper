using SQLite;

namespace DiabetesHelper.Models;

public class GlucoseReading
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public double ValueMgDl { get; set; }

    public string? Notes { get; set; }
}
