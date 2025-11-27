using RaspberryIoT.Application.Models;

namespace RaspberryIoT.Application.Repositories;

public interface ISensorEventRepository
{
    Task<bool> CreateAsync(SensorEvent sensorEvent, CancellationToken token = default);
    Task<SensorEvent?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<IEnumerable<SensorEvent>> GetAllAsync(EventType? eventTypeFilter = null, CancellationToken token = default);
    Task<IEnumerable<SensorEvent>> GetNewEventsSinceRowIdAsync(int sinceRowId, CancellationToken token = default);
}
