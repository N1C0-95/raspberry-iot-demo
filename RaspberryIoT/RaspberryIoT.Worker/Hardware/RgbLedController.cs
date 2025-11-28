using System.Device.Gpio;

namespace RaspberryIoT.Worker.Hardware;

/// <summary>
/// RGB LED Controller - Gestisce LED RGB per indicare stato sistema
/// Verde   = Tutti i sensori OK
/// Rosso   = Sensore 1 in errore
/// Blu     = Sensore 2 in errore
/// Viola   = Entrambi sensori in errore (futuro)
/// </summary>
public class RgbLedController : IDisposable
{
    private readonly GpioController _gpio;
    private readonly ILogger<RgbLedController> _logger;

    public RgbLedController(GpioController gpio, ILogger<RgbLedController> logger)
    {
        _gpio = gpio;
        _logger = logger;
        Initialize();
    }

    private void Initialize()
    {
        _gpio.OpenPin(GpioPins.RgbRed, PinMode.Output);
        _gpio.OpenPin(GpioPins.RgbGreen, PinMode.Output);
        _gpio.OpenPin(GpioPins.RgbBlue, PinMode.Output);
        
        // Stato iniziale: Verde (tutti OK)
        SetGreen();
        
        _logger.LogInformation("RGB LED Controller initialized");
    }

    /// <summary>
    /// Verde = Sistema OK (tutti sensori online)
    /// </summary>
    public void SetGreen()
    {
        _gpio.Write(GpioPins.RgbRed, PinValue.Low);
        _gpio.Write(GpioPins.RgbGreen, PinValue.High);
        _gpio.Write(GpioPins.RgbBlue, PinValue.Low);
        _logger.LogDebug("RGB LED: Green (System OK)");
    }

    /// <summary>
    /// Rosso = Sensore 1 in errore
    /// </summary>
    public void SetRed()
    {
        _gpio.Write(GpioPins.RgbRed, PinValue.High);
        _gpio.Write(GpioPins.RgbGreen, PinValue.Low);
        _gpio.Write(GpioPins.RgbBlue, PinValue.Low);
        _logger.LogWarning("RGB LED: Red (Sensor 1 Error)");
    }

    /// <summary>
    /// Blu = Sensore 2 in errore
    /// </summary>
    public void SetBlue()
    {
        _gpio.Write(GpioPins.RgbRed, PinValue.Low);
        _gpio.Write(GpioPins.RgbGreen, PinValue.Low);
        _gpio.Write(GpioPins.RgbBlue, PinValue.High);
        _logger.LogWarning("RGB LED: Blue (Sensor 2 Error)");
    }

    /// <summary>
    /// Viola = Entrambi sensori in errore (futuro)
    /// </summary>
    public void SetPurple()
    {
        _gpio.Write(GpioPins.RgbRed, PinValue.High);
        _gpio.Write(GpioPins.RgbGreen, PinValue.Low);
        _gpio.Write(GpioPins.RgbBlue, PinValue.High);
        _logger.LogError("RGB LED: Purple (Both Sensors Error)");
    }

    /// <summary>
    /// Giallo = Warning (opzionale)
    /// </summary>
    public void SetYellow()
    {
        _gpio.Write(GpioPins.RgbRed, PinValue.High);
        _gpio.Write(GpioPins.RgbGreen, PinValue.High);
        _gpio.Write(GpioPins.RgbBlue, PinValue.Low);
        _logger.LogWarning("RGB LED: Yellow (Warning)");
    }

    /// <summary>
    /// Spento = Nessun LED acceso
    /// </summary>
    public void TurnOff()
    {
        _gpio.Write(GpioPins.RgbRed, PinValue.Low);
        _gpio.Write(GpioPins.RgbGreen, PinValue.Low);
        _gpio.Write(GpioPins.RgbBlue, PinValue.Low);
        _logger.LogDebug("RGB LED: Off");
    }

    /// <summary>
    /// Imposta colore in base a SensorId
    /// </summary>
    public void SetColorForSensor(string sensorId)
    {
        switch (sensorId)
        {
            case "SENSOR-001":
                SetRed();
                break;
            case "SENSOR-002":
                SetBlue();
                break;
            default:
                SetYellow(); // Colore warning per sensori sconosciuti
                break;
        }
    }

    public void Dispose()
    {
        TurnOff();
        _gpio.ClosePin(GpioPins.RgbRed);
        _gpio.ClosePin(GpioPins.RgbGreen);
        _gpio.ClosePin(GpioPins.RgbBlue);
    }
}
