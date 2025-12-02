using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Repositories;

namespace RaspberryIoT.Application.Services;

public class SensorOrchestrator : ISensorOrchestrator
{
    private readonly ISensorStatusRepository _statusRepository;
    private readonly ISensorEventRepository _eventRepository;

    public SensorOrchestrator(
        ISensorStatusRepository statusRepository,
        ISensorEventRepository eventRepository)
    {
        _statusRepository = statusRepository;
        _eventRepository = eventRepository;
    }

    public async Task HandleErrorDetectedAsync(string sensorId, string triggeredBy, CancellationToken token = default)
    {
        var now = DateTime.UtcNow;

        // 1. Crea il nuovo status (Error)
        var status = new SensorStatus
        {
            Id = Guid.NewGuid(),
            SensorId = sensorId,
            Status = SensorStatusEnum.Error,
            LedColor = LedColor.Red,
            ChangedOn = triggeredBy,
            Timestamp = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _statusRepository.CreateAsync(status, token);

        // 2. Crea l'evento correlato
        var sensorEvent = new SensorEvent
        {
            Id = Guid.NewGuid(),
            EventType = EventType.ErrorDetected,
            Status = SensorStatusEnum.Error.ToString(),
            TriggeredBy = triggeredBy,
            Timestamp = now,
            CreatedAt = now
        };

        await _eventRepository.CreateAsync(sensorEvent, token);
    }

    public async Task HandleRebootStartedAsync(string sensorId, string triggeredBy, CancellationToken token = default)
    {
        var now = DateTime.UtcNow;

        // 1. Crea il nuovo status (Rebooting)
        var status = new SensorStatus
        {
            Id = Guid.NewGuid(),
            SensorId = sensorId,
            Status = SensorStatusEnum.Rebooting,
            LedColor = LedColor.Off,
            ChangedOn = triggeredBy,
            Timestamp = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _statusRepository.CreateAsync(status, token);

        // 2. Crea l'evento correlato
        var sensorEvent = new SensorEvent
        {
            Id = Guid.NewGuid(),
            EventType = EventType.RebootStarted,
            Status = SensorStatusEnum.Rebooting.ToString(),
            TriggeredBy = triggeredBy,
            Timestamp = now,
            CreatedAt = now
        };

        await _eventRepository.CreateAsync(sensorEvent, token);
    }

    public async Task HandleRebootCompletedAsync(string sensorId, string triggeredBy, CancellationToken token = default)
    {
        var now = DateTime.UtcNow;

        // 1. Crea il nuovo status (Online)
        var status = new SensorStatus
        {
            Id = Guid.NewGuid(),
            SensorId = sensorId,
            Status = SensorStatusEnum.Online,
            LedColor = LedColor.Green,
            ChangedOn = triggeredBy,
            Timestamp = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _statusRepository.CreateAsync(status, token);

        // 2. Crea l'evento correlato
        var sensorEvent = new SensorEvent
        {
            Id = Guid.NewGuid(),
            EventType = EventType.RebootCompleted,
            Status = SensorStatusEnum.Online.ToString(),
            TriggeredBy = triggeredBy,
            Timestamp = now,
            CreatedAt = now
        };

        await _eventRepository.CreateAsync(sensorEvent, token);
    }
}
