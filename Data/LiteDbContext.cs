using LiteDB;

namespace DiabetesHelper.Data;

public class LiteDbContext
{
    // Bump this and add an entry to Migrations whenever a change to a model isn't purely additive
    // (i.e. anything beyond adding a new nullable/optional property) - see CLAUDE.md "Schema
    // evolution" for what counts as safe vs. needs a migration.
    private const int CurrentSchemaVersion = 1;

    private static readonly (int Version, Action<LiteDatabase> Apply)[] Migrations = Array.Empty<(int, Action<LiteDatabase>)>();

    public LiteDatabase Database { get; }

    public LiteDbContext()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "diabeteshelper.litedb");
        Database = new LiteDatabase(dbPath);

        foreach (var migration in Migrations)
        {
            if (Database.UserVersion < migration.Version)
            {
                migration.Apply(Database);
                Database.UserVersion = migration.Version;
            }
        }

        Database.UserVersion = CurrentSchemaVersion;
    }
}
