namespace MapTalkie.Tests
{
    public class DBContstants
    {
        public const string TestingDatabaseHost = "localhost";
        public const string TestingDatabaseUsername = "admin";
        public const string TestingDatabasePassword = "admin";

        public static string DatabaseConnectionString(string databaseName)
            =>
                $"Host={TestingDatabaseHost};Database={databaseName};Username={TestingDatabaseUsername};Password={TestingDatabasePassword}";
    }
}