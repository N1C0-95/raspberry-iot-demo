namespace RaspberryIoT.Worker.Hardware;

/// <summary>
/// GPIO Pin mapping (BCM numbering)
/// </summary>
public static class GpioPins
{
    /// <summary>
    /// LED RGB - Red Pin (BCM 17, Physical Pin 11)
    /// </summary>
    public const int RgbRed = 17;
    
    /// <summary>
    /// LED RGB - Green Pin (BCM 27, Physical Pin 13)
    /// </summary>
    public const int RgbGreen = 27;
    
    /// <summary>
    /// LED RGB - Blue Pin (BCM 22, Physical Pin 15)
    /// </summary>
    public const int RgbBlue = 22;
    
    /// <summary>
    /// Pulsante Sensore 1 (BCM 23, Physical Pin 16)
    /// Pull-Up Interno: Pressed = LOW, Released = HIGH
    /// </summary>
    public const int Button1 = 23;
    
    /// <summary>
    /// Pulsante Sensore 2 (BCM 24, Physical Pin 18)
    /// Pull-Up Interno: Pressed = LOW, Released = HIGH
    /// </summary>
    public const int Button2 = 24;
}
