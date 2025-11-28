using System.Device.Gpio;

namespace RaspberryIoT.Worker.Hardware;

public class LedController : IDisposable
{
    private readonly GpioController _gpio;
    private readonly ILogger<LedController> _logger;

    public LedController(GpioController gpio, ILogger<LedController> logger)
    {
        _gpio = gpio;
        _logger = logger;
        Initialize();
    }

    private void Initialize()
    {
        _gpio.OpenPin(GpioPins.LedGreen, PinMode.Output);
        _gpio.OpenPin(GpioPins.LedRed, PinMode.Output);
        
        // Stato iniziale: Verde ON, Rosso OFF
        SetNormalState();
        
        _logger.LogInformation("LED Controller initialized");
    }

    public void SetNormalState()
    {
        _gpio.Write(GpioPins.LedGreen, PinValue.High);
        _gpio.Write(GpioPins.LedRed, PinValue.Low);
        _logger.LogDebug("LED State: Normal (Green ON, Red OFF)");
    }

    public void SetErrorState()
    {
        _gpio.Write(GpioPins.LedGreen, PinValue.Low);
        _gpio.Write(GpioPins.LedRed, PinValue.High);
        _logger.LogWarning("LED State: Error (Green OFF, Red ON)");
    }

    public void TurnOffAll()
    {
        _gpio.Write(GpioPins.LedGreen, PinValue.Low);
        _gpio.Write(GpioPins.LedRed, PinValue.Low);
        _logger.LogDebug("All LEDs turned OFF");
    }

    public void Dispose()
    {
        TurnOffAll();
        _gpio.ClosePin(GpioPins.LedGreen);
        _gpio.ClosePin(GpioPins.LedRed);
    }
}
