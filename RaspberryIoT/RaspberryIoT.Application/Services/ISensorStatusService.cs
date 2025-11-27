using RaspberryIoT.Application.Models;
using RaspberryIoT.Contracts.Requests;
using RaspberryIoT.Contracts.Responses;

namespace RaspberryIoT.Application.Services;

public interface ISensorStatusService
{
    Task<SensorStatusResponse?> GetByIdAsync(Guid id, CancellationToken token = default);
    Task<IEnumerable<SensorStatusResponse>> GetAllAsync(CancellationToken token = default);
    Task<SensorStatusResponse?> GetCurrentBySensorIdAsync(string sensorId, CancellationToken token = default);
    Task<SensorStatusPollResponse> GetNewStatusesSinceRowIdAsync(int sinceRowId, CancellationToken token = default);
    Task<SensorStatusResponse> CreateAsync(UpdateSensorStatusRequest request, CancellationToken token = default);
    Task<bool> UpdateAsync(Guid id, UpdateSensorStatusRequest request, CancellationToken token = default);
    Task<SensorStatusResponse> ForceErrorAsync(ForceErrorRequest request, CancellationToken token = default);
    Task<SensorStatusResponse> ForceRebootAsync(ForceRebootRequest request, CancellationToken token = default);
}
