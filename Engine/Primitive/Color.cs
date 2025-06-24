namespace RawDraw.Engine.Primitive;

public struct Color
{
    public byte r;
    public byte g;
    public byte b;

    public Color(byte r, byte g, byte b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }

    public static Color FromHex(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        if (hex.Length != 6)
            throw new FormatException("Hex color must be 6 characters.");

        return new Color(
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16)
        );
    }
    
    public static string ToHex(Color color)
    {
        return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
    }

    public static Color Black { get => new Color(0, 0, 0); }
    public static Color White { get => new Color(255, 255, 255); }
    public static Color Gray { get => new Color(50, 50, 50); }
    public static Color Red { get => new Color(255, 0, 0); }
    public static Color Green { get => new Color(0, 255, 0); }
    public static Color Blue { get => new Color(0, 0, 255); }
}