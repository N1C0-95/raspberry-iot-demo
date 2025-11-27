using Microsoft.AspNetCore.Mvc;
using RaspberryIoT.Application.Services;
using RaspberryIoT.Contracts;
using RaspberryIoT.Contracts.Requests;

namespace RaspberryIoT.Api.Controllers;

[ApiController]
public class SensorEventsController : ControllerBase
{
    private readonly ISensorEventService _sensorEventService;

    public SensorEventsController(ISensorEventService sensorEventService)
    {
        _sensorEventService = sensorEventService;
    }

    [HttpGet(ApiRoutes.SensorEvents.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] string? eventType = null, CancellationToken token = default)
    {
        var events = await _sensorEventService.GetAllAsync(eventType, token);
        return Ok(events);
    }

    [HttpGet(ApiRoutes.SensorEvents.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken token)
    {
        var sensorEvent = await _sensorEventService.GetByIdAsync(id, token);
        
        if (sensorEvent == null)
        {
            return NotFound();
        }

        return Ok(sensorEvent);
    }

    [HttpGet(ApiRoutes.SensorEvents.Poll)]
    public async Task<IActionResult> Poll([FromQuery] int sinceRowId = 0, CancellationToken token = default)
    {
        var response = await _sensorEventService.GetNewEventsSinceRowIdAsync(sinceRowId, token);
        return Ok(response);
    }

    [HttpPost(ApiRoutes.SensorEvents.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSensorEventRequest request, CancellationToken token)
    {
        var sensorEvent = await _sensorEventService.CreateAsync(request, token);
        return CreatedAtAction(nameof(GetById), new { id = sensorEvent.Id }, sensorEvent);
    }
}
