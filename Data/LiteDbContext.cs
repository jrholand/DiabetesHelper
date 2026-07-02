using LiteDB;

namespace DiabetesHelper.Data;

public class LiteDbContext
{
    public LiteDatabase Database { get; }

    public LiteDbContext()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "diabeteshelper.litedb");
        Database = new LiteDatabase(dbPath);
    }
}
