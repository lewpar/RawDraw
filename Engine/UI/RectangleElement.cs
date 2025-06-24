using System.Xml.Serialization;

using RawDraw.Engine.Drawing;
using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.UI;

public class RectangleElement : UIElement
{
    [XmlAttribute("x")]
    public int X { get; set; }
    
    [XmlAttribute("y")]
    public int Y { get; set; }
    
    [XmlAttribute("width")]
    public int Width { get; set; }
    
    [XmlAttribute("height")]
    public int Height { get; set; }

    public override void Draw(FrameBuffer buffer)
    {
        buffer.FillRect(X, Y, Width, Height, Color.White);
    }
}