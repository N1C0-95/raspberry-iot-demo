using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Services;

namespace RaspberryIoT.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISensorOrchestrator _orchestrator;
    private readonly IConfiguration _configuration;
    private LedColor _previousLedState = LedColor.Green;
    private readonly Random _random = new();

    public Worker(
        ILogger<Worker> logger,
        ISensorOrchestrator orchestrator,
        IConfiguration configuration)
    {
        _logger = logger;
        _orchestrator = orchestrator;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sensorId = _configuration.GetValue<string>("Worker:SensorId") ?? "RASPBERRY-DEMO-001";
        var pollingInterval = _configuration.GetValue<int>("Worker:PollingIntervalMs", 5000);

        _logger.LogInformation("Worker started - SensorId: {SensorId}, Polling: {Interval}ms", sensorId, pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Simula lettura GPIO (random: 80% Green, 20% Red)
                var currentLedState = SimulateGpioRead();

                if (currentLedState != _previousLedState)
                {
                    _logger.LogWarning("LED State Changed: {Previous} -> {Current}", _previousLedState, currentLedState);

                    if (currentLedState == LedColor.Red)
                    {
                        // Errore rilevato!
                        _logger.LogError("Error Detected! Calling Orchestrator...");
                        await _orchestrator.HandleErrorDetectedAsync(sensorId, "worker", stoppingToken);
                        _logger.LogInformation("Error status and event created successfully");
                    }
                    else if (currentLedState == LedColor.Off)
                    {
                        // Reboot rilevato!
                        _logger.LogWarning("Reboot Started! Calling Orchestrator...");
                        await _orchestrator.HandleRebootStartedAsync(sensorId, "worker", stoppingToken);
                        _logger.LogInformation("Reboot status and event created successfully");
                    }
                    else
                    {
                        _logger.LogInformation("LED back to normal (Green)");
                    }

                    _previousLedState = currentLedState;
                }

                await Task.Delay(pollingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Worker execution");
                await Task.Delay(1000, stoppingToken); // Wait before retry
            }
        }
    }

    private LedColor SimulateGpioRead()
    {
        // Simula lettura GPIO con valori random
        // 80% Green (normale), 15% Red (errore), 5% Off (reboot)
        var value = _random.Next(100);

        return value switch
        {
            < 80 => LedColor.Green,  // 80% normale
            < 95 => LedColor.Red,    // 15% errore
            _ => LedColor.Off        // 5% reboot
        };
    }
}
