using System.Data;
using Microsoft.Data.Sqlite;
using RaspberryIoT.Application.Database;

namespace RaspberryIoT.Infrastructure.Database;

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(token);
        return connection;
    }
}
