using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Repositories;
using RaspberryIoT.Contracts.Requests;
using RaspberryIoT.Contracts.Responses;

namespace RaspberryIoT.Application.Services;

public class SensorEventService : ISensorEventService
{
    private readonly ISensorEventRepository _repository;

    public SensorEventService(ISensorEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<SensorEventResponse?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var sensorEvent = await _repository.GetByIdAsync(id, token);
        return sensorEvent != null ? MapToResponse(sensorEvent) : null;
    }

    public async Task<IEnumerable<SensorEventResponse>> GetAllAsync(string? eventTypeFilter = null, CancellationToken token = default)
    {
        EventType? filter = null;
        if (!string.IsNullOrWhiteSpace(eventTypeFilter) && Enum.TryParse<EventType>(eventTypeFilter, out var parsedFilter))
        {
            filter = parsedFilter;
        }

        var sensorEvents = await _repository.GetAllAsync(filter, token);
        return sensorEvents.Select(MapToResponse);
    }

    public async Task<SensorEventPollResponse> GetNewEventsSinceRowIdAsync(int sinceRowId, CancellationToken token = default)
    {
        var sensorEvents = await _repository.GetNewEventsSinceRowIdAsync(sinceRowId, token);
        return new SensorEventPollResponse
        {
            Data = sensorEvents.Select(MapToResponse).ToList()
        };
    }

    public async Task<SensorEventResponse> CreateAsync(CreateSensorEventRequest request, CancellationToken token = default)
    {
        var now = DateTime.UtcNow;
        var sensorEvent = new SensorEvent
        {
            Id = Guid.NewGuid(),
            EventType = Enum.Parse<EventType>(request.EventType),
            Status = request.Status,
            TriggeredBy = request.TriggeredBy,
            Timestamp = now,
            CreatedAt = now
        };

        await _repository.CreateAsync(sensorEvent, token);
        return MapToResponse(sensorEvent);
    }

    private static SensorEventResponse MapToResponse(SensorEvent sensorEvent)
    {
        return new SensorEventResponse
        {
            RowId = sensorEvent.RowId,
            Id = sensorEvent.Id,
            EventType = sensorEvent.EventType.ToString(),
            Status = sensorEvent.Status,
            TriggeredBy = sensorEvent.TriggeredBy,
            Timestamp = sensorEvent.Timestamp,
            CreatedAt = sensorEvent.CreatedAt
        };
    }
}
