namespace RaspberryIoT.Contracts.Requests;

public class ForceErrorRequest
{
    public required string SensorId { get; init; }
    public required string TriggeredBy { get; init; }
}
