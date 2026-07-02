namespace DiabetesHelper.Models;

public enum InsulinType
{
    Bolus,
    Basal
}

public class InsulinDose
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime EffectiveDateUtc { get; set; } = DateTime.UtcNow;

    public double Units { get; set; }

    public InsulinType Type { get; set; }

    public string? Notes { get; set; }
}
