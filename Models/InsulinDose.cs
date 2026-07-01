using SQLite;

namespace DiabetesHelper.Models;

public enum InsulinType
{
    Bolus,
    Basal
}

public class InsulinDose
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public double Units { get; set; }

    public InsulinType Type { get; set; }

    public string? Notes { get; set; }
}
