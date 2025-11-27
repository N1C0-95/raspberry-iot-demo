using RaspberryIoT.Application.Models;

namespace RaspberryIoT.Application.Services;

public interface ISensorOrchestrator
{
    Task HandleErrorDetectedAsync(string sensorId, string triggeredBy, CancellationToken token = default);
    Task HandleRebootStartedAsync(string sensorId, string triggeredBy, CancellationToken token = default);
}
