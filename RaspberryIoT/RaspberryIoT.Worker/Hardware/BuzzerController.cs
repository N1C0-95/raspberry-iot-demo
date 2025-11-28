using System.Device.Gpio;

namespace RaspberryIoT.Worker.Hardware;

/// <summary>
/// Controller for Active Buzzer (simple ON/OFF, no frequency control)
/// Pattern: Christmas rhythm beeps
/// </summary>
public class BuzzerController : IDisposable
{
    private readonly GpioController _gpio;
    private readonly ILogger<BuzzerController> _logger;
    private CancellationTokenSource? _melodyCts;

    // Jingle Bells Rhythm Pattern (ON/OFF durations in milliseconds)
    // Pattern simula il ritmo di "Jingle Bells" con beep semplici
    private static readonly int[] BeepPattern = 
    { 
        150, 100,  // Beep corto, pausa corta (E)
        150, 100,  // Beep corto, pausa corta (E)
        300, 200,  // Beep lungo, pausa media (E - lunga)
        
        150, 100,  // Beep corto, pausa corta (E)
        150, 100,  // Beep corto, pausa corta (E)
        300, 200,  // Beep lungo, pausa media (E - lunga)
        
        150, 100,  // E
        150, 100,  // G
        150, 100,  // C
        150, 100,  // D
        400, 500   // E - finale lungo con pausa
    };

    public BuzzerController(GpioController gpio, ILogger<BuzzerController> logger)
    {
        _gpio = gpio;
        _logger = logger;
        Initialize();
    }

    private void Initialize()
    {
        _gpio.OpenPin(GpioPins.Buzzer, PinMode.Output);
        _gpio.Write(GpioPins.Buzzer, PinValue.Low);
        _logger.LogInformation("🔊 Active Buzzer Controller initialized");
    }

    public void StartMelody()
    {
        Stop();
        _melodyCts = new CancellationTokenSource();
        
        Task.Run(async () => await PlayPatternLoopAsync(_melodyCts.Token), _melodyCts.Token);
        _logger.LogInformation("🎵 Christmas beep pattern started");
    }

    public void Stop()
    {
        _melodyCts?.Cancel();
        _melodyCts?.Dispose();
        _melodyCts = null;
        _gpio.Write(GpioPins.Buzzer, PinValue.Low);
        _logger.LogInformation("🔇 Buzzer stopped");
    }

    private async Task PlayPatternLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Suona il pattern ritmico
            for (int i = 0; i < BeepPattern.Length - 1 && !cancellationToken.IsCancellationRequested; i += 2)
            {
                // Beep ON
                _gpio.Write(GpioPins.Buzzer, PinValue.High);
                await Task.Delay(BeepPattern[i], cancellationToken);
                
                // Beep OFF (pausa)
                _gpio.Write(GpioPins.Buzzer, PinValue.Low);
                await Task.Delay(BeepPattern[i + 1], cancellationToken);
            }

            // Pausa tra ripetizioni del pattern
            await Task.Delay(2000, cancellationToken);
        }
    }

    /// <summary>
    /// Suona 3 beep rapidi per indicare reboot/recovery
    /// </summary>
    public async Task PlayRebootBeepsAsync()
    {
        for (int i = 0; i < 3; i++)
        {
            _gpio.Write(GpioPins.Buzzer, PinValue.High);
            await Task.Delay(100);
            _gpio.Write(GpioPins.Buzzer, PinValue.Low);
            await Task.Delay(100);
        }
        _logger.LogInformation("🔔 Reboot beeps completed");
    }

    public void Dispose()
    {
        Stop();
        _gpio.ClosePin(GpioPins.Buzzer);
    }
}
