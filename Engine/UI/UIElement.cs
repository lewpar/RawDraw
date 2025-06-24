using System.Xml.Serialization;
using RawDraw.Engine.Drawing;

namespace RawDraw.Engine.UI;

[XmlInclude(typeof(FrameElement))]
[XmlInclude(typeof(RectangleElement))]
public abstract class UIElement
{
    [XmlElement("Grid", typeof(GridElement))]
    [XmlElement("Rectangle", typeof(RectangleElement))]
    public List<UIElement> Children { get; } = new List<UIElement>();
    
    [XmlIgnore]
    public UIElement? Parent { get; set; }
    
    public virtual void Draw(FrameBuffer buffer) { }
    public virtual void Update(float deltaTimeMs) { }
} 