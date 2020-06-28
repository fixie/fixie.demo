namespace ContactList.Tests
{
    public class TestSettings
    {
        public DatabaseSettings? Database { get; set; }

        public class DatabaseSettings
        {
            public string? ConnectionString { get; set; }
        }
    }
}