namespace Loly.App.Db.Settings
{
    public interface ILolyDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }

    public class LolyDatabaseSettings : ILolyDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}