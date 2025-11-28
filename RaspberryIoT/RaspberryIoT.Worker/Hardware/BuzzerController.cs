using System.Diagnostics;

namespace RaspberryIoT.Worker.Hardware;

/// <summary>
/// Audio Controller - Plays audio files through Bluetooth speaker (Jabra)
/// Uses ALSA (aplay) for audio playback
/// </summary>
public class BuzzerController : IDisposable
{
    private readonly ILogger<BuzzerController> _logger;
    private readonly IConfiguration _configuration;
    private Process? _audioProcess;
    private CancellationTokenSource? _melodyCts;
    
    // Path del file audio (configurabile)
    private readonly string _audioFilePath;
    private readonly bool _loopAudio;
    private readonly string _audioDevice;

    public BuzzerController(ILogger<BuzzerController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Leggi configurazione
        _audioFilePath = configuration.GetValue<string>("Audio:FilePath") 
            ?? "/home/nicolo/sounds/jingle-bells.wav";
        _loopAudio = configuration.GetValue<bool>("Audio:Loop", true);
        _audioDevice = configuration.GetValue<string>("Audio:Device") ?? "default";
        
        _logger.LogInformation("🔊 Audio Controller initialized - File: {AudioFile}, Device: {Device}", 
            _audioFilePath, _audioDevice);
    }

    public void StartMelody()
    {
        Stop();
        _melodyCts = new CancellationTokenSource();
        
        if (_loopAudio)
        {
            // Modalità loop continuo
            Task.Run(async () => await PlayLoopAsync(_melodyCts.Token), _melodyCts.Token);
        }
        else
        {
            // Play singolo
            PlayAudioFile();
        }
        
        _logger.LogInformation("🎵 Christmas music started");
    }

    public void Stop()
    {
        _melodyCts?.Cancel();
        _melodyCts?.Dispose();
        _melodyCts = null;
        
        StopAudioProcess();
        _logger.LogInformation("🔇 Music stopped");
    }

    private async Task PlayLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                PlayAudioFile();
                
                // Attendi che il processo finisca o venga cancellato
                if (_audioProcess != null)
                {
                    await _audioProcess.WaitForExitAsync(cancellationToken);
                }
                
                // Pausa tra ripetizioni
                await Task.Delay(2000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Cancellazione normale
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error playing audio file");
                await Task.Delay(5000, cancellationToken); // Retry dopo 5 secondi
            }
        }
    }

    private void PlayAudioFile()
    {
        try
        {
            // Verifica esistenza file
            if (!File.Exists(_audioFilePath))
            {
                _logger.LogWarning("⚠️  Audio file not found: {FilePath}", _audioFilePath);
                _logger.LogInformation("💡 Tip: Download Jingle Bells with: wget https://www.soundjay.com/christmas/sounds/jingle-bells-1.mp3 -O {FilePath}", _audioFilePath);
                return;
            }

            StopAudioProcess();
            
            _audioProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "aplay",
                    Arguments = _audioDevice == "default" 
                        ? $"-q \"{_audioFilePath}\"" 
                        : $"-D {_audioDevice} -q \"{_audioFilePath}\"", // -D specifica device
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            _audioProcess.Start();
            _logger.LogDebug("Audio playback started: {Command} {Args}", 
                _audioProcess.StartInfo.FileName, 
                _audioProcess.StartInfo.Arguments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start audio playback");
        }
    }

    private void StopAudioProcess()
    {
        if (_audioProcess != null && !_audioProcess.HasExited)
        {
            try
            {
                _audioProcess.Kill();
                _audioProcess.WaitForExit(1000);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error stopping audio process");
            }
        }
        
        _audioProcess?.Dispose();
        _audioProcess = null;
    }

    public void Dispose()
    {
        Stop();
    }
}
