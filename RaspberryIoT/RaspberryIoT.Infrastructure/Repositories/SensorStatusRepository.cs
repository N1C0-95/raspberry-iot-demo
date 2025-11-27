using Dapper;
using RaspberryIoT.Application.Database;
using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Repositories;

namespace RaspberryIoT.Infrastructure.Repositories;

public class SensorStatusRepository : ISensorStatusRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SensorStatusRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(SensorStatus sensorStatus, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.ExecuteAsync(
            """
            INSERT INTO SensorStatus (Id, SensorId, Status, LedColor, ChangedOn, Timestamp, CreatedAt, UpdatedAt)
            VALUES (@Id, @SensorId, @Status, @LedColor, @ChangedOn, @Timestamp, @CreatedAt, @UpdatedAt)
            """,
            new
            {
                Id = sensorStatus.Id.ToString(),
                sensorStatus.SensorId,
                Status = sensorStatus.Status.ToString(),
                LedColor = sensorStatus.LedColor.ToString(),
                sensorStatus.ChangedOn,
                sensorStatus.Timestamp,
                sensorStatus.CreatedAt,
                sensorStatus.UpdatedAt
            });

        return result > 0;
    }

    public async Task<SensorStatus?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.QuerySingleOrDefaultAsync<SensorStatusDto>(
            "SELECT * FROM SensorStatus WHERE Id = @Id",
            new { Id = id.ToString() });

        return result != null ? MapToEntity(result) : null;
    }

    public async Task<IEnumerable<SensorStatus>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var results = await connection.QueryAsync<SensorStatusDto>(
            "SELECT * FROM SensorStatus ORDER BY RowId DESC");

        return results.Select(MapToEntity);
    }

    public async Task<SensorStatus?> GetCurrentBySensorIdAsync(string sensorId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.QuerySingleOrDefaultAsync<SensorStatusDto>(
            """
            SELECT * FROM SensorStatus 
            WHERE SensorId = @SensorId 
            ORDER BY Timestamp DESC 
            LIMIT 1
            """,
            new { SensorId = sensorId });

        return result != null ? MapToEntity(result) : null;
    }

    public async Task<IEnumerable<SensorStatus>> GetNewStatusesSinceRowIdAsync(int sinceRowId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var results = await connection.QueryAsync<SensorStatusDto>(
            """
            SELECT * FROM SensorStatus 
            WHERE RowId > @SinceRowId 
            ORDER BY RowId DESC
            """,
            new { SinceRowId = sinceRowId });

        return results.Select(MapToEntity);
    }

    public async Task<bool> UpdateAsync(Guid id, SensorStatus sensorStatus, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.ExecuteAsync(
            """
            UPDATE SensorStatus 
            SET SensorId = @SensorId,
                Status = @Status,
                LedColor = @LedColor,
                ChangedOn = @ChangedOn,
                Timestamp = @Timestamp,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """,
            new
            {
                Id = id.ToString(),
                sensorStatus.SensorId,
                Status = sensorStatus.Status.ToString(),
                LedColor = sensorStatus.LedColor.ToString(),
                sensorStatus.ChangedOn,
                sensorStatus.Timestamp,
                sensorStatus.UpdatedAt
            });

        return result > 0;
    }

    private static SensorStatus MapToEntity(SensorStatusDto dto)
    {
        return new SensorStatus
        {
            RowId = dto.RowId,
            Id = Guid.Parse(dto.Id),
            SensorId = dto.SensorId,
            Status = Enum.Parse<SensorStatusEnum>(dto.Status),
            LedColor = Enum.Parse<LedColor>(dto.LedColor),
            ChangedOn = dto.ChangedOn,
            Timestamp = dto.Timestamp,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }

    private class SensorStatusDto
    {
        public int RowId { get; set; }
        public string Id { get; set; } = string.Empty;
        public string SensorId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LedColor { get; set; } = string.Empty;
        public string ChangedOn { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
