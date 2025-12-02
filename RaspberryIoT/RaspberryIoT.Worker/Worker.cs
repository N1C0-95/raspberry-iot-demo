using RaspberryIoT.Application.Models;
using RaspberryIoT.Application.Services;
using RaspberryIoT.Worker.Hardware;
using System.Net.Http.Json;
using RaspberryIoT.Contracts.Responses;

namespace RaspberryIoT.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISensorOrchestrator _orchestrator;
    private readonly IConfiguration _configuration;
    private readonly RgbLedController _rgbLedController;
    private readonly BuzzerController _buzzerController;
    private readonly ButtonMonitor _buttonMonitor;
    private readonly HttpClient _httpClient;
    
    private bool _isInErrorState = false;
    private bool _isInRebootState = false;
    private string? _currentErrorSensorId = null;
    private SensorStatusEnum? _lastKnownStatus = null;
    private DateTime _lastStatusCheck = DateTime.MinValue;

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
        _httpClient = new HttpClient();
        
        // Registra evento pressione pulsante
        _buttonMonitor.ButtonPressed += OnButtonPressed;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollingInterval = _configuration.GetValue<int>("Worker:PollingIntervalMs", 100);
        var statusCheckInterval = _configuration.GetValue<int>("Worker:StatusCheckIntervalMs", 2000);

        _logger.LogInformation("Christmas Worker started with multi-sensor support");
        
        // Avvia la melodia natalizia e LED verde (tutto OK)
        _buzzerController.StartMelody();
        _rgbLedController.SetGreen();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Monitora i pulsanti (polling veloce per reattività)
                _buttonMonitor.CheckState();
                
                // Controlla lo status API solo se in errore e ogni X secondi
                if (_isInErrorState && !_isInRebootState &&
                    (DateTime.UtcNow - _lastStatusCheck).TotalMilliseconds >= statusCheckInterval)
                {
                    await CheckForStatusChangeAsync(stoppingToken);
                    _lastStatusCheck = DateTime.UtcNow;
                }

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
        _currentErrorSensorId = buttonEventArgs.SensorId;
        _lastKnownStatus = SensorStatusEnum.Error;
        
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
            _currentErrorSensorId = null;
            _lastKnownStatus = null;
        }
    }

    private async Task CheckForStatusChangeAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_currentErrorSensorId))
        {
            return;
        }

        try
        {
            var apiBaseUrl = _configuration.GetValue<string>("Worker:ApiBaseUrl", "http://localhost:5000");
            var response = await _httpClient.GetAsync(
                $"{apiBaseUrl}/api/sensor-status/{_currentErrorSensorId}/current",
                token
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to check status from API: {StatusCode}", response.StatusCode);
                return;
            }

            var statusResponse = await response.Content.ReadFromJsonAsync<SensorStatusResponse>(cancellationToken: token);
            
            if (statusResponse == null)
            {
                _logger.LogWarning("Received null status response from API");
                return;
            }

            var currentStatus = Enum.Parse<SensorStatusEnum>(statusResponse.Status);

            // Rileva transizione Error → Rebooting (comando reboot arrivato!)
            if (_lastKnownStatus == SensorStatusEnum.Error &&
                currentStatus == SensorStatusEnum.Rebooting)
            {
                _logger.LogInformation("🔄 Reboot command detected from API for sensor {SensorId}", _currentErrorSensorId);
                await HandleRebootAsync(token);
            }

            _lastKnownStatus = currentStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check status from API");
        }
    }

    private async Task HandleRebootAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_currentErrorSensorId))
        {
            return;
        }

        try
        {
            _logger.LogInformation("🔄 Starting reboot process for sensor {SensorId}", _currentErrorSensorId);

            _isInRebootState = true;

            // Reboot completato → Scrivi Online nel DB
            await _orchestrator.HandleRebootCompletedAsync(_currentErrorSensorId, "worker_auto", token);

            // Ripristina stato normale
            _rgbLedController.SetGreen();
            _buzzerController.StartMelody();

            _isInErrorState = false;
            _isInRebootState = false;
            _lastKnownStatus = SensorStatusEnum.Online;
            _currentErrorSensorId = null;

            _logger.LogInformation("✅ System back online!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete reboot process");
            // In caso di errore, reset flags per permettere retry
            _isInRebootState = false;
        }
    }

    public override void Dispose()
    {
        _buttonMonitor.ButtonPressed -= OnButtonPressed;
        _httpClient?.Dispose();
        base.Dispose();
    }
}
