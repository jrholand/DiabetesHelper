using SQLite;

namespace DiabetesHelper.Models;

// Stored in the local SQLite db (see LocalDatabase) so keys survive independently of platform
// secure-storage quirks - on this dev machine's MSIX sideload workflow, SecureStorage was found
// not to survive a package reinstall, while the db file does. Unlike the other Models/ types,
// this table is NOT meant to ever be included if/when a cloud-sync backend is added later -
// these are secrets, not data to sync.
public class ApiKeyEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public DateTime EnteredUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; }

    [Ignore]
    public DateTime EnteredLocal => EnteredUtc.ToLocalTime();

    [Ignore]
    public bool IsNotActive => !IsActive;

    [Ignore]
    public string MaskedKey => Key.Length <= 4
        ? new string('•', Key.Length)
        : new string('•', Key.Length - 4) + Key[^4..];
}
