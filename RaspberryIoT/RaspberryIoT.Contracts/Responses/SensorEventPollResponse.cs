namespace RaspberryIoT.Contracts.Responses;

public class SensorEventPollResponse
{
    public required List<SensorEventResponse> Data { get; init; }
}
