using Dapper;
using RaspberryIoT.Application.Database;
using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Repositories;

namespace RaspberryIoT.Infrastructure.Repositories;

public class SensorEventRepository : ISensorEventRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SensorEventRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(SensorEvent sensorEvent, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.ExecuteAsync(
            """
            INSERT INTO SensorEvents (Id, EventType, Status, TriggeredBy, Timestamp, CreatedAt)
            VALUES (@Id, @EventType, @Status, @TriggeredBy, @Timestamp, @CreatedAt)
            """,
            new
            {
                Id = sensorEvent.Id.ToString(),
                EventType = sensorEvent.EventType.ToString(),
                sensorEvent.Status,
                sensorEvent.TriggeredBy,
                sensorEvent.Timestamp,
                sensorEvent.CreatedAt
            });

        return result > 0;
    }

    public async Task<SensorEvent?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.QuerySingleOrDefaultAsync<SensorEventDto>(
            "SELECT * FROM SensorEvents WHERE Id = @Id",
            new { Id = id.ToString() });

        return result != null ? MapToEntity(result) : null;
    }

    public async Task<IEnumerable<SensorEvent>> GetAllAsync(EventType? eventTypeFilter = null, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        string sql = "SELECT * FROM SensorEvents";
        object? parameters = null;

        if (eventTypeFilter.HasValue)
        {
            sql += " WHERE EventType = @EventType";
            parameters = new { EventType = eventTypeFilter.Value.ToString() };
        }

        sql += " ORDER BY RowId DESC";

        var results = await connection.QueryAsync<SensorEventDto>(sql, parameters);
        return results.Select(MapToEntity);
    }

    public async Task<IEnumerable<SensorEvent>> GetNewEventsSinceRowIdAsync(int sinceRowId, CancellationToken token = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(token);
        
        var results = await connection.QueryAsync<SensorEventDto>(
            """
            SELECT * FROM SensorEvents 
            WHERE RowId > @SinceRowId 
            ORDER BY RowId DESC
            """,
            new { SinceRowId = sinceRowId });

        return results.Select(MapToEntity);
    }

    private static SensorEvent MapToEntity(SensorEventDto dto)
    {
        return new SensorEvent
        {
            RowId = dto.RowId,
            Id = Guid.Parse(dto.Id),
            EventType = Enum.Parse<EventType>(dto.EventType),
            Status = dto.Status,
            TriggeredBy = dto.TriggeredBy,
            Timestamp = dto.Timestamp,
            CreatedAt = dto.CreatedAt
        };
    }

    private class SensorEventDto
    {
        public int RowId { get; set; }
        public string Id { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TriggeredBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
