using RawDraw.Engine.Drawing;
using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.UI;

public class FrameElement : UIElement
{
    public FrameElement(int x, int y, int width, int height) : base(x, y, width, height)
    {
    }

    public override void OnDraw(FrameBuffer buffer)
    {
        buffer.FillRect(Bounds, Color.Gray);
    }
}