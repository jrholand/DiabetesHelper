using LiteDB;

namespace DiabetesHelper.Models;

// Stored in the local LiteDB store (see LiteDbContext) so keys survive independently of platform
// secure-storage quirks - on this dev machine's MSIX sideload workflow, SecureStorage was found
// not to survive a package reinstall, while the db file does. Unlike the other Models/ types,
// this collection is NOT meant to ever be included if/when a cloud-sync backend is added later -
// these are secrets, not data to sync. Also unlike GlucoseReading/InsulinDose/Meal, this has no
// CreatedAtUtc/EffectiveDateUtc split - it's a secret with an entry date, not a loggable diabetes
// record a user would ever want to backdate.
public class ApiKeyEntry
{
    public int Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public DateTime EnteredUtc { get; set; } = DateTime.UtcNow;

    // Not persisted - exactly one entry across the whole vault is ever active, tracked by a
    // single id pointer (see LiteDbApiKeyVaultService), and set on this instance at read time.
    [BsonIgnore]
    public bool IsActive { get; set; }

    [BsonIgnore]
    public DateTime EnteredLocal => EnteredUtc.ToLocalTime();

    [BsonIgnore]
    public bool IsNotActive => !IsActive;

    [BsonIgnore]
    public string MaskedKey => Key.Length <= 4
        ? new string('•', Key.Length)
        : new string('•', Key.Length - 4) + Key[^4..];
}
