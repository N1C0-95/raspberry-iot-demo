namespace RaspberryIoT.Contracts.Responses;

public class SensorEventResponse
{
    public required int RowId { get; init; }
    public required Guid Id { get; init; }
    public required string EventType { get; init; }
    public required string Status { get; init; }
    public required string TriggeredBy { get; init; }
    public required DateTime Timestamp { get; init; }
    public required DateTime CreatedAt { get; init; }
}
