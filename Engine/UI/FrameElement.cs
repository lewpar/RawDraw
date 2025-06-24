using System.Xml.Serialization;

using RawDraw.Engine.Drawing;

namespace RawDraw.Engine.UI;

[XmlRoot("Frame", Namespace = XmlParser.NAMESPACE)]
public class FrameElement : UIElement
{
    public override void Draw(FrameBuffer buffer)
    {
        foreach (var child in Children)
        {
            child.Draw(buffer);
        }
    }

    public override void Update(float deltaTimeMs)
    {
        foreach (var child in Children)
        {
            child.Update(deltaTimeMs);
        }
    }
}