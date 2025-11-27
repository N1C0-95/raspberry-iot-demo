namespace RaspberryIoT.Contracts.Requests;

public class UpdateSensorStatusRequest
{
    public required string SensorId { get; init; }
    public required string Status { get; init; }
    public required string LedColor { get; init; }
    public required string ChangedOn { get; init; }
}
