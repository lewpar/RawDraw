using System.Drawing;

namespace RawDraw;

public class RawRectangle
{
    public required PointF Position { get; set; }
    public required Point Size { get; set; }
    public required PointF Velocity { get; set; }
    public required Color Color { get; set; }

    public RawRectangle()
    {

    }
}