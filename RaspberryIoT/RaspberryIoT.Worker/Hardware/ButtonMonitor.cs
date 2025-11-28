using System.Device.Gpio;

namespace RaspberryIoT.Worker.Hardware;

public class ButtonMonitor : IDisposable
{
    private readonly GpioController _gpio;
    private readonly ILogger<ButtonMonitor> _logger;
    private bool _lastState = true; // Pull-up: true = not pressed

    public event EventHandler? ButtonPressed;

    public ButtonMonitor(GpioController gpio, ILogger<ButtonMonitor> logger)
    {
        _gpio = gpio;
        _logger = logger;
        Initialize();
    }

    private void Initialize()
    {
        // Pull-Up interno: HIGH quando non premuto, LOW quando premuto
        _gpio.OpenPin(GpioPins.Button, PinMode.InputPullUp);
        _logger.LogInformation("Button Monitor initialized (Pull-Up mode)");
    }

    public void CheckState()
    {
        var currentState = _gpio.Read(GpioPins.Button) == PinValue.High;

        // Rileva transizione da HIGH a LOW (pulsante premuto)
        if (_lastState && !currentState)
        {
            _logger.LogWarning("⚠️ BUTTON PRESSED! Error simulation triggered");
            ButtonPressed?.Invoke(this, EventArgs.Empty);
        }

        _lastState = currentState;
    }

    public void Dispose()
    {
        _gpio.ClosePin(GpioPins.Button);
    }
}
