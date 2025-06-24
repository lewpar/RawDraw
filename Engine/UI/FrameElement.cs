using RawDraw.Engine.Drawing;
using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.UI;

public class FrameElement : UIElement
{
    private Color _color;

    public FrameElement(int x, int y, int width, int height, Color color) : base(x, y, width, height)
    {
        _color = color;
    }

    public override void Draw(FrameBuffer buffer)
    {
        buffer.FillRect(Bounds, _color);
    }
}