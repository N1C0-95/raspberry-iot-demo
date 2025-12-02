using Microsoft.AspNetCore.Mvc;
using RaspberryIoT.Application.Services;
using RaspberryIoT.Contracts;
using RaspberryIoT.Contracts.Requests;

namespace RaspberryIoT.Api.Controllers;

[ApiController]
public class SensorStatusController : ControllerBase
{
    private readonly ISensorStatusService _sensorStatusService;
    private readonly ISensorOrchestrator _orchestrator;

    public SensorStatusController(
        ISensorStatusService sensorStatusService,
        ISensorOrchestrator orchestrator)
    {
        _sensorStatusService = sensorStatusService;
        _orchestrator = orchestrator;
    }

    [HttpGet(ApiRoutes.SensorStatus.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var statuses = await _sensorStatusService.GetAllAsync(token);
        return Ok(statuses);
    }

    [HttpGet(ApiRoutes.SensorStatus.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken token)
    {
        var status = await _sensorStatusService.GetByIdAsync(id, token);
        
        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    [HttpGet(ApiRoutes.SensorStatus.GetCurrent)]
    public async Task<IActionResult> GetCurrent([FromRoute] string sensorId, CancellationToken token)
    {
        var status = await _sensorStatusService.GetCurrentBySensorIdAsync(sensorId, token);
        
        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    [HttpGet(ApiRoutes.SensorStatus.Poll)]
    public async Task<IActionResult> Poll([FromQuery] int sinceRowId = 0, [FromQuery] string id = null, CancellationToken token = default)
    {
        var response = await _sensorStatusService.GetNewStatusesSinceRowIdAsync(sinceRowId, token);
        return Ok(response);
    }

    [HttpPut(ApiRoutes.SensorStatus.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSensorStatusRequest request, CancellationToken token)
    {
        var updated = await _sensorStatusService.UpdateAsync(id, request, token);
        
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost(ApiRoutes.SensorStatus.ForceError)]
    public async Task<IActionResult> ForceError([FromBody] ForceErrorRequest request, CancellationToken token)
    {
        await _orchestrator.HandleErrorDetectedAsync(request.SensorId, request.TriggeredBy, token);
        
        // Recupera lo status appena creato per la risposta
        var status = await _sensorStatusService.GetCurrentBySensorIdAsync(request.SensorId, token);
        return CreatedAtAction(nameof(GetById), new { id = status!.Id }, status);
    }

    [HttpPost(ApiRoutes.SensorStatus.ForceReboot)]
    public async Task<IActionResult> ForceReboot([FromBody] ForceRebootRequest request, CancellationToken token)
    {
        await _orchestrator.HandleRebootStartedAsync(request.SensorId, request.TriggeredBy, token);
        
        // Recupera lo status appena creato per la risposta
        var status = await _sensorStatusService.GetCurrentBySensorIdAsync(request.SensorId, token);
        return CreatedAtAction(nameof(GetById), new { id = status!.Id }, status);
    }
}
