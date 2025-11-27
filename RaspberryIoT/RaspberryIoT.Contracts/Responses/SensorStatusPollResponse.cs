namespace RaspberryIoT.Contracts.Responses;

public class SensorStatusPollResponse
{
    public required List<SensorStatusResponse> Data { get; init; }
}
