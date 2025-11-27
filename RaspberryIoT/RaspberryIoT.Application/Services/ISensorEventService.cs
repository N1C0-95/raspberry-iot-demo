using RaspberryIoT.Application.Models;
using RaspberryIoT.Contracts.Requests;
using RaspberryIoT.Contracts.Responses;

namespace RaspberryIoT.Application.Services;

public interface ISensorEventService
{
    Task<SensorEventResponse?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<IEnumerable<SensorEventResponse>> GetAllAsync(string? eventTypeFilter = null, CancellationToken token = default);
    Task<SensorEventPollResponse> GetNewEventsSinceRowIdAsync(int sinceRowId, CancellationToken token = default);
    Task<SensorEventResponse> CreateAsync(CreateSensorEventRequest request, CancellationToken token = default);
}
