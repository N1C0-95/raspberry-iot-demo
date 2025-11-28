namespace RaspberryIoT.Worker.Hardware;

/// <summary>
/// GPIO Pin mapping (BCM numbering)
/// </summary>
public static class GpioPins
{
    /// <summary>
    /// LED Verde - Sistema OK (BCM 17, Physical Pin 11)
    /// </summary>
    public const int LedGreen = 17;
    
    /// <summary>
    /// LED Rosso - Sistema in Errore (BCM 27, Physical Pin 13)
    /// </summary>
    public const int LedRed = 27;
    
    /// <summary>
    /// Passive Buzzer - Melodia Natalizia (BCM 22, Physical Pin 15)
    /// </summary>
    public const int Buzzer = 22;
    
    /// <summary>
    /// Pulsante - Simulazione Errore (BCM 23, Physical Pin 16)
    /// Pull-Up Interno: Pressed = LOW, Released = HIGH
    /// </summary>
    public const int Button = 23;
}
