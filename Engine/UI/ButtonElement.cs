using System.Xml.Serialization;

using RawDraw.Engine.Drawing;
using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.UI;

public class ButtonElement : UIElement
{
    [XmlAttribute("text")]
    public string? Text { get; set; }

    [XmlAttribute("border-size")]
    public int BorderSize { get; set; } = 3;

    public Color BorderColor { get; set; }
    
    [XmlAttribute("border-color")]
    public string BorderColorHex
    {
        get => Color.ToHex(BorderColor);
        set => BorderColor = string.IsNullOrWhiteSpace(value) ? Color.Gray : Color.FromHex(value);
    }
    
    public Color Foreground { get; set; }

    [XmlAttribute("foreground")]
    public string ForegroundHex
    {
        get => Color.ToHex(Foreground);
        set => Foreground = string.IsNullOrWhiteSpace(value) ? Color.Black : Color.FromHex(value);
    }
    
    public Color Background { get; set; }

    [XmlAttribute("background")]
    public string BackgroundHex
    {
        get => Color.ToHex(Background);
        set => Background = string.IsNullOrWhiteSpace(value) ? Color.White : Color.FromHex(value);
    }
    
    [XmlAttribute("x")]
    public int X { get; set; }
    
    [XmlAttribute("y")]
    public int Y { get; set; }
    
    [XmlAttribute("width")]
    public int Width { get; set; }
    
    [XmlAttribute("height")]
    public int Height { get; set; }
    
    [XmlAttribute("padding")]
    public int Padding { get; set; }

    public override void Draw(FrameBuffer buffer)
    {
        buffer.FillRect(X, Y, (Width > 0 ? Width : MeasureText(Text ?? "")) + (Padding * 2), 8 + (Padding * 2), Background);
        buffer.DrawRect(X, Y, Width + (Padding * 2), Height + (Padding * 2), BorderSize, BorderColor);

        if (!string.IsNullOrWhiteSpace(Text))
        {
            buffer.DrawText(X + Padding, Y + Padding, Text, Foreground);   
        }
    }
}