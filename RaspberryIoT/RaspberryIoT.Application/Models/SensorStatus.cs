namespace RaspberryIoT.Application.Models;

public class SensorStatus
{
    public int RowId { get; set; }
    public Guid Id { get; set; }
    public required string SensorId { get; set; }
    public SensorStatusEnum Status { get; set; }
    public LedColor LedColor { get; set; }
    public required string ChangedOn { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
