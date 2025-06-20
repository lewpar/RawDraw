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

    public static Color Black { get => new Color(0, 0, 0); }
    public static Color White { get => new Color(255, 255, 255); }
    public static Color Gray { get => new Color(50, 50, 50); }
    public static Color Red { get => new Color(255, 0, 0); }
    public static Color Green { get => new Color(0, 255, 0); }
    public static Color Blue { get => new Color(0, 0, 255); }
}