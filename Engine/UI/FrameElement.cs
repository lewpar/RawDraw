using System.Xml.Serialization;

using RawDraw.Engine.Drawing;

namespace RawDraw.Engine.UI;

[XmlRoot("Frame", Namespace = XmlParser.NAMESPACE)]
public class FrameElement : UIElement
{
}