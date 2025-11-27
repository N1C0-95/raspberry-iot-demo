namespace RaspberryIoT.Contracts.Responses;

public class SensorStatusResponse
{
    public required int RowId { get; init; }
    public required Guid Id { get; init; }
    public required string SensorId { get; init; }
    public required string Status { get; init; }
    public required string LedColor { get; init; }
    public required string ChangedOn { get; init; }
    public required DateTime Timestamp { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
