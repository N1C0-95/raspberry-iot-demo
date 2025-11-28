using System.Device.Gpio;

namespace RaspberryIoT.Worker.Hardware;

/// <summary>
/// Controller for Passive Buzzer - Jingle Bells melody
/// </summary>
public class BuzzerController : IDisposable
{
    private readonly GpioController _gpio;
    private readonly ILogger<BuzzerController> _logger;
    private CancellationTokenSource? _melodyCts;

    // Jingle Bells - Versione semplificata (11 note)
    private static readonly int[] JingleBellsNotes = 
    { 
        659, 659, 659,  // E E E
        659, 659, 659,  // E E E
        659, 784, 523, 587, 659  // E G C D E
    };

    private static readonly int[] JingleBellsDurations = 
    { 
        250, 250, 500,  // E E E (corta corta lunga)
        250, 250, 500,  // E E E
        250, 250, 250, 250, 1000  // E G C D E (finale lungo)
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
        _logger.LogInformation("Buzzer Controller initialized");
    }

    public void StartMelody()
    {
        Stop();
        _melodyCts = new CancellationTokenSource();
        
        Task.Run(async () => await PlayMelodyLoopAsync(_melodyCts.Token), _melodyCts.Token);
        _logger.LogInformation("🎵 Jingle Bells melody started");
    }

    public void Stop()
    {
        _melodyCts?.Cancel();
        _melodyCts?.Dispose();
        _melodyCts = null;
        _gpio.Write(GpioPins.Buzzer, PinValue.Low);
        _logger.LogInformation("🔇 Melody stopped");
    }

    private async Task PlayMelodyLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            for (int i = 0; i < JingleBellsNotes.Length && !cancellationToken.IsCancellationRequested; i++)
            {
                await PlayToneAsync(JingleBellsNotes[i], JingleBellsDurations[i], cancellationToken);
                await Task.Delay(50, cancellationToken); // Pausa tra note
            }

            // Pausa tra ripetizioni melodia
            await Task.Delay(1000, cancellationToken);
        }
    }

    private async Task PlayToneAsync(int frequency, int durationMs, CancellationToken cancellationToken)
    {
        var period = 1000000 / frequency; // Periodo in microsecondi
        var halfPeriod = period / 2;
        var cycles = (durationMs * 1000) / period;

        for (int i = 0; i < cycles && !cancellationToken.IsCancellationRequested; i++)
        {
            _gpio.Write(GpioPins.Buzzer, PinValue.High);
            await Task.Delay(TimeSpan.FromMicroseconds(halfPeriod), cancellationToken);
            
            _gpio.Write(GpioPins.Buzzer, PinValue.Low);
            await Task.Delay(TimeSpan.FromMicroseconds(halfPeriod), cancellationToken);
        }
    }

    public void Dispose()
    {
        Stop();
        _gpio.ClosePin(GpioPins.Buzzer);
    }
}
