using Dapper;
using RaspberryIoT.Application.Database;

namespace RaspberryIoT.Infrastructure.Database;

public class DbInitializer : IDbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS SensorStatus (
                RowId INTEGER PRIMARY KEY AUTOINCREMENT,
                Id TEXT NOT NULL UNIQUE,
                SensorId TEXT NOT NULL,
                Status TEXT NOT NULL,
                LedColor TEXT NOT NULL,
                ChangedOn TEXT NOT NULL,
                Timestamp DATETIME NOT NULL,
                CreatedAt DATETIME NOT NULL,
                UpdatedAt DATETIME NOT NULL
            );
            """);

        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS SensorEvents (
                RowId INTEGER PRIMARY KEY AUTOINCREMENT,
                Id TEXT NOT NULL UNIQUE,
                EventType TEXT NOT NULL,
                Status TEXT NOT NULL,
                TriggeredBy TEXT NOT NULL,
                Timestamp DATETIME NOT NULL,
                CreatedAt DATETIME NOT NULL
            );
            """);
    }
}
