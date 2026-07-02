namespace DiabetesHelper.Models;

public class GlucoseReading
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime EffectiveDateUtc { get; set; } = DateTime.UtcNow;

    public double ValueMgDl { get; set; }

    public string? Notes { get; set; }
}
