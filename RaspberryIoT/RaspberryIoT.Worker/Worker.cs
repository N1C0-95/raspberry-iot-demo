using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Services;
using RaspberryIoT.Worker.Hardware;

namespace RaspberryIoT.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISensorOrchestrator _orchestrator;
    private readonly IConfiguration _configuration;
    private readonly LedController _ledController;
    private readonly BuzzerController _buzzerController;
    private readonly ButtonMonitor _buttonMonitor;
    
    private bool _isInErrorState = false;
    private bool _isInRebootState = false;

    public Worker(
        ILogger<Worker> logger,
        ISensorOrchestrator orchestrator,
        IConfiguration configuration,
        LedController ledController,
        BuzzerController buzzerController,
        ButtonMonitor buttonMonitor)
    {
        _logger = logger;
        _orchestrator = orchestrator;
        _configuration = configuration;
        _ledController = ledController;
        _buzzerController = buzzerController;
        _buttonMonitor = buttonMonitor;
        
        // Registra evento pressione pulsante
        _buttonMonitor.ButtonPressed += OnButtonPressed;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sensorId = _configuration.GetValue<string>("Worker:SensorId") ?? "RASPBERRY-DEMO-001";
        var pollingInterval = _configuration.GetValue<int>("Worker:PollingIntervalMs", 1000); // Check button ogni 1 secondo

        _logger.LogInformation("Christmas Worker started - SensorId: {SensorId}", sensorId);
        
        // Avvia la melodia natalizia
        _buzzerController.StartMelody();
        _ledController.SetNormalState();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Monitora il pulsante
                _buttonMonitor.CheckState();
                
                // TODO: Qui in futuro controlleremo il DB per comandi di reboot dall'API
                // if (CheckForRebootCommand()) { HandleReboot(); }

                await Task.Delay(pollingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Worker execution");
                await Task.Delay(1000, stoppingToken);
            }
        }
        
        // Cleanup quando il worker si ferma
        _buzzerController.Stop();
        _ledController.TurnOffAll();
    }

    private async void OnButtonPressed(object? sender, EventArgs e)
    {
        if (_isInErrorState)
        {
            _logger.LogWarning("Already in error state, ignoring button press");
            return;
        }

        _isInErrorState = true;
        
        try
        {
            var sensorId = _configuration.GetValue<string>("Worker:SensorId") ?? "RASPBERRY-DEMO-001";
            
            _logger.LogError(" ERROR DETECTED! Stopping melody and activating red LED");
            
            // Stop melodia e attiva LED rosso
            _buzzerController.Stop();
            _ledController.SetErrorState();
            
            // Scrivi nel DB tramite orchestrator
            await _orchestrator.HandleErrorDetectedAsync(sensorId, "button_press", CancellationToken.None);
            
            _logger.LogInformation("Error status and event saved to database");
            _logger.LogWarning("System in error state - waiting for API reboot command...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle button press");
            _isInErrorState = false; // Reset per permettere retry
        }
    }

    public override void Dispose()
    {
        _buttonMonitor.ButtonPressed -= OnButtonPressed;
        base.Dispose();
    }
}
