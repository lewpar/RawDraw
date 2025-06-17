using System.Drawing;

namespace RawDraw;

public class RawRectangle
{
    public required Point Position { get; set; }
    public required Point Size { get; set; }
    public required Point Velocity { get; set; }
    public required Color Color { get; set; }

    public RawRectangle()
    {

    }
}