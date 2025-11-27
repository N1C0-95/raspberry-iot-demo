using RaspberryIoT.Application.Models;

namespace RaspberryIoT.Application.Repositories;

public interface ISensorStatusRepository
{
    Task<bool> CreateAsync(SensorStatus sensorStatus, CancellationToken token = default);
    Task<SensorStatus?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<IEnumerable<SensorStatus>> GetAllAsync(CancellationToken token = default);
    Task<SensorStatus?> GetCurrentBySensorIdAsync(string sensorId, CancellationToken token = default);
    Task<IEnumerable<SensorStatus>> GetNewStatusesSinceRowIdAsync(int sinceRowId, CancellationToken token = default);
    Task<bool> UpdateAsync(Guid id, SensorStatus sensorStatus, CancellationToken token = default);
}
