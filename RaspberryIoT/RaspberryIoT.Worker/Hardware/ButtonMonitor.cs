using System.Device.Gpio;

namespace RaspberryIoT.Worker.Hardware;

public class ButtonPressedEventArgs : EventArgs
{
    public string SensorId { get; set; } = string.Empty;
    public int ButtonPin { get; set; }
}

public class ButtonMonitor : IDisposable
{
    private readonly GpioController _gpio;
    private readonly ILogger<ButtonMonitor> _logger;
    private readonly IConfiguration _configuration;
    private bool _lastStateButton1 = true;
    private bool _lastStateButton2 = true;
    
    private readonly Dictionary<int, string> _buttonToSensorMap = new();

    public event EventHandler<ButtonPressedEventArgs>? ButtonPressed;

    public ButtonMonitor(GpioController gpio, ILogger<ButtonMonitor> logger, IConfiguration configuration)
    {
        _gpio = gpio;
        _logger = logger;
        _configuration = configuration;
        Initialize();
    }

    private void Initialize()
    {
        // Carica configurazione sensori
        var sensors = _configuration.GetSection("Sensors").Get<List<SensorConfig>>() ?? new List<SensorConfig>();
        
        foreach (var sensor in sensors)
        {
            _gpio.OpenPin(sensor.ButtonPin, PinMode.InputPullUp);
            _buttonToSensorMap[sensor.ButtonPin] = sensor.Id;
            _logger.LogInformation("Button Monitor: Pin {Pin} → Sensor {SensorId}", sensor.ButtonPin, sensor.Id);
        }
        
        _logger.LogInformation("Button Monitor initialized ({Count} sensors, Pull-Up mode)", sensors.Count);
    }

    public void CheckState()
    {
        // Check Button 1 (GPIO 23)
        if (_buttonToSensorMap.ContainsKey(GpioPins.Button1))
        {
            var currentState1 = _gpio.Read(GpioPins.Button1) == PinValue.High;
            if (_lastStateButton1 && !currentState1)
            {
                var sensorId = _buttonToSensorMap[GpioPins.Button1];
                _logger.LogWarning("⚠️ BUTTON 1 PRESSED! Sensor: {SensorId}", sensorId);
                ButtonPressed?.Invoke(this, new ButtonPressedEventArgs { SensorId = sensorId, ButtonPin = GpioPins.Button1 });
            }
            _lastStateButton1 = currentState1;
        }
        
        // Check Button 2 (GPIO 24)
        if (_buttonToSensorMap.ContainsKey(GpioPins.Button2))
        {
            var currentState2 = _gpio.Read(GpioPins.Button2) == PinValue.High;
            if (_lastStateButton2 && !currentState2)
            {
                var sensorId = _buttonToSensorMap[GpioPins.Button2];
                _logger.LogWarning("⚠️ BUTTON 2 PRESSED! Sensor: {SensorId}", sensorId);
                ButtonPressed?.Invoke(this, new ButtonPressedEventArgs { SensorId = sensorId, ButtonPin = GpioPins.Button2 });
            }
            _lastStateButton2 = currentState2;
        }
    }

    public void Dispose()
    {
        foreach (var pin in _buttonToSensorMap.Keys)
        {
            _gpio.ClosePin(pin);
        }
    }
}

public class SensorConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ButtonPin { get; set; }
}
