using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Repositories;
using RaspberryIoT.Contracts.Requests;
using RaspberryIoT.Contracts.Responses;

namespace RaspberryIoT.Application.Services;

public class SensorStatusService : ISensorStatusService
{
    private readonly ISensorStatusRepository _repository;

    public SensorStatusService(ISensorStatusRepository repository)
    {
        _repository = repository;
    }

    public async Task<SensorStatusResponse?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        var sensorStatus = await _repository.GetByIdAsync(id, token);
        return sensorStatus != null ? MapToResponse(sensorStatus) : null;
    }

    public async Task<IEnumerable<SensorStatusResponse>> GetAllAsync(CancellationToken token = default)
    {
        var sensorStatuses = await _repository.GetAllAsync(token);
        return sensorStatuses.Select(MapToResponse);
    }

    public async Task<SensorStatusResponse?> GetCurrentBySensorIdAsync(string sensorId, CancellationToken token = default)
    {
        var sensorStatus = await _repository.GetCurrentBySensorIdAsync(sensorId, token);
        return sensorStatus != null ? MapToResponse(sensorStatus) : null;
    }

    public async Task<SensorStatusPollResponse> GetNewStatusesSinceRowIdAsync(int sinceRowId, CancellationToken token = default)
    {
        var sensorStatuses = await _repository.GetNewStatusesSinceRowIdAsync(sinceRowId, token);
        return new SensorStatusPollResponse
        {
            Data = sensorStatuses.Select(MapToResponse).ToList()
        };
    }

    public async Task<SensorStatusResponse> CreateAsync(UpdateSensorStatusRequest request, CancellationToken token = default)
    {
        var now = DateTime.UtcNow;
        var sensorStatus = new SensorStatus
        {
            Id = Guid.NewGuid(),
            SensorId = request.SensorId,
            Status = Enum.Parse<SensorStatusEnum>(request.Status),
            LedColor = Enum.Parse<LedColor>(request.LedColor),
            ChangedOn = request.ChangedOn,
            Timestamp = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.CreateAsync(sensorStatus, token);
        return MapToResponse(sensorStatus);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateSensorStatusRequest request, CancellationToken token = default)
    {
        var sensorStatus = new SensorStatus
        {
            SensorId = request.SensorId,
            Status = Enum.Parse<SensorStatusEnum>(request.Status),
            LedColor = Enum.Parse<LedColor>(request.LedColor),
            ChangedOn = request.ChangedOn,
            Timestamp = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _repository.UpdateAsync(id, sensorStatus, token);
    }

    public async Task<SensorStatusResponse> ForceErrorAsync(ForceErrorRequest request, CancellationToken token = default)
    {
        var updateRequest = new UpdateSensorStatusRequest
        {
            SensorId = request.SensorId,
            Status = SensorStatusEnum.Error.ToString(),
            LedColor = LedColor.Red.ToString(),
            ChangedOn = request.TriggeredBy
        };

        return await CreateAsync(updateRequest, token);
    }

    public async Task<SensorStatusResponse> ForceRebootAsync(ForceRebootRequest request, CancellationToken token = default)
    {
        var updateRequest = new UpdateSensorStatusRequest
        {
            SensorId = request.SensorId,
            Status = SensorStatusEnum.Rebooting.ToString(),
            LedColor = LedColor.Off.ToString(),
            ChangedOn = request.TriggeredBy
        };

        return await CreateAsync(updateRequest, token);
    }

    private static SensorStatusResponse MapToResponse(SensorStatus sensorStatus)
    {
        return new SensorStatusResponse
        {
            RowId = sensorStatus.RowId,
            Id = sensorStatus.Id,
            SensorId = sensorStatus.SensorId,
            Status = sensorStatus.Status.ToString(),
            LedColor = sensorStatus.LedColor.ToString(),
            ChangedOn = sensorStatus.ChangedOn,
            Timestamp = sensorStatus.Timestamp,
            CreatedAt = sensorStatus.CreatedAt,
            UpdatedAt = sensorStatus.UpdatedAt
        };
    }
}
