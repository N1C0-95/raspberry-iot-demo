namespace RaspberryIoT.Application.Models;

public class SensorEvent
{
    public int RowId { get; set; }
    public Guid Id { get; set; }
    public EventType EventType { get; set; }
    public required string Status { get; set; }
    public required string TriggeredBy { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }
}
