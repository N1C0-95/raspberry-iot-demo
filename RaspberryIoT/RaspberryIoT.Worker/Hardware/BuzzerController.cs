using System.Device.Gpio;

namespace RaspberryIoT.Worker.Hardware;

public class BuzzerController : IDisposable
{
    private readonly GpioController _gpio;
    private readonly ILogger<BuzzerController> _logger;
    private CancellationTokenSource? _melodyCts;

    // Jingle Bells - Melodia completa con note più accurate
    private static readonly int[] JingleBellsNotes = 
    { 
        659, 659, 659, 0,     // E E E (pausa)
        659, 659, 659, 0,     // E E E (pausa)
        659, 784, 523, 587, 659, 0,  // E G C D E (pausa)
        698, 698, 698, 698,   // F F F F
        698, 659, 659, 659,   // F E E E
        659, 587, 587, 659,   // E D D E
        587, 784, 0           // D G (pausa finale)
    };

    private static readonly int[] JingleBellsDurations = 
    { 
        200, 200, 400, 100,   // E E E (pausa)
        200, 200, 400, 100,   // E E E (pausa)
        200, 200, 200, 200, 400, 200,  // E G C D E (pausa)
        200, 200, 200, 200,   // F F F F
        200, 200, 200, 100,   // F E E E
        200, 200, 200, 200,   // E D D E
        400, 400, 300         // D G (pausa finale)
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
                if (JingleBellsNotes[i] == 0)
                {
                    // Pausa (silenzio)
                    await Task.Delay(JingleBellsDurations[i], cancellationToken);
                }
                else
                {
                    PlayToneBlocking(JingleBellsNotes[i], JingleBellsDurations[i]);
                }
                
                if (cancellationToken.IsCancellationRequested) break;
            }

            // Pausa tra ripetizioni melodia
            await Task.Delay(2000, cancellationToken);
        }
    }

    private void PlayToneBlocking(int frequency, int durationMs)
    {
        if (frequency <= 0) return;
        
        var periodMicros = 1_000_000.0 / frequency;
        var halfPeriodMicros = periodMicros / 2.0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var targetTicks = durationMs * TimeSpan.TicksPerMillisecond;

        while (stopwatch.ElapsedTicks < targetTicks)
        {
            _gpio.Write(GpioPins.Buzzer, PinValue.High);
            SpinWaitMicroseconds(halfPeriodMicros);
            
            _gpio.Write(GpioPins.Buzzer, PinValue.Low);
            SpinWaitMicroseconds(halfPeriodMicros);
        }
        
        _gpio.Write(GpioPins.Buzzer, PinValue.Low);
    }

    private static void SpinWaitMicroseconds(double microseconds)
    {
        var targetTicks = (long)(microseconds * TimeSpan.TicksPerMillisecond / 1000.0);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        while (stopwatch.ElapsedTicks < targetTicks)
        {
            // Spin-wait attivo per timing preciso
            Thread.SpinWait(10);
        }
    }

    public void Dispose()
    {
        Stop();
        _gpio.ClosePin(GpioPins.Buzzer);
    }
}
