using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Services;
using RaspberryIoT.Worker.Hardware;

namespace RaspberryIoT.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISensorOrchestrator _orchestrator;
    private readonly IConfiguration _configuration;
    private readonly RgbLedController _rgbLedController;
    private readonly BuzzerController _buzzerController;
    private readonly ButtonMonitor _buttonMonitor;
    
    private bool _isInErrorState = false;
    private bool _isInRebootState = false;

    public Worker(
        ILogger<Worker> logger,
        ISensorOrchestrator orchestrator,
        IConfiguration configuration,
        RgbLedController rgbLedController,
        BuzzerController buzzerController,
        ButtonMonitor buttonMonitor)
    {
        _logger = logger;
        _orchestrator = orchestrator;
        _configuration = configuration;
        _rgbLedController = rgbLedController;
        _buzzerController = buzzerController;
        _buttonMonitor = buttonMonitor;
        
        // Registra evento pressione pulsante
        _buttonMonitor.ButtonPressed += OnButtonPressed;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollingInterval = _configuration.GetValue<int>("Worker:PollingIntervalMs", 100); // Check button ogni 100ms (10x al secondo)

        _logger.LogInformation("🎄 Christmas Worker started with multi-sensor support");
        
        // Avvia la melodia natalizia e LED verde (tutto OK)
        _buzzerController.StartMelody();
        _rgbLedController.SetGreen();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Monitora i pulsanti (polling veloce per reattività)
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
        _rgbLedController.TurnOff();
    }

    private async void OnButtonPressed(object? sender, EventArgs e)
    {
        if (_isInErrorState)
        {
            _logger.LogWarning("Already in error state, ignoring button press");
            return;
        }

        // Cast EventArgs to ButtonPressedEventArgs
        if (e is not ButtonPressedEventArgs buttonEventArgs)
        {
            _logger.LogError("Invalid event args type");
            return;
        }

        _isInErrorState = true;
        
        try
        {
            var sensorId = buttonEventArgs.SensorId;
            
            _logger.LogError("⚠️ ERROR DETECTED! Sensor: {SensorId}, Button Pin: {ButtonPin}", 
                sensorId, buttonEventArgs.ButtonPin);
            
            // Stop melodia e attiva LED specifico per sensore
            _buzzerController.Stop();
            _rgbLedController.SetColorForSensor(sensorId);
            
            // Scrivi nel DB tramite orchestrator
            await _orchestrator.HandleErrorDetectedAsync(sensorId, "button_press", CancellationToken.None);
            
            _logger.LogInformation("Error status and event saved to database for sensor {SensorId}", sensorId);
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
