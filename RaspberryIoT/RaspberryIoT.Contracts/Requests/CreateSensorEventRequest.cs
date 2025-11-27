namespace RaspberryIoT.Contracts.Requests;

public class CreateSensorEventRequest
{
    public required string EventType { get; init; }
    public required string Status { get; init; }
    public required string TriggeredBy { get; init; }
}
