namespace RaspberryIoT.Contracts.Requests;

public class ForceRebootRequest
{
    public required string SensorId { get; init; }
    public required string TriggeredBy { get; init; }
}
