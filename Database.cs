using Npgsql;

namespace MenuWithDatabase;

public class Database
{

    private readonly string _host = "42.10.162.204";
    private readonly string _port = "5435";
    private readonly string _username = "postgres";
    private readonly string _password = "InsecureGorillaPukes33";
    private readonly string _database = "holidaymaker-cloud";

    private NpgsqlDataSource _connection;

    public NpgsqlDataSource Connection()
    {
        return _connection;
    }
    public Database()
    {
        string connectionString = $"Host={_host};Port={_port};Username={_username};Password={_password};Database={_database}";
        _connection = NpgsqlDataSource.Create(connectionString);
    }
}