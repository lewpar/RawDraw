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

    [XmlAttribute("background")]
    public string ForegroundHex
    {
        get => Color.ToHex(Foreground);
        set => Foreground = string.IsNullOrWhiteSpace(value) ? Color.Black : Color.FromHex(value);
    }
    
    public Color Background { get; set; }

    [XmlAttribute("foreground")]
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

    public override void Draw(FrameBuffer buffer)
    {
        buffer.FillRect(X, Y, Width, Height, Background);
        buffer.DrawRect(X, Y, Width, Height, BorderSize, BorderColor);

        if (!string.IsNullOrWhiteSpace(Text))
        {
            buffer.DrawText(X, Y, Text, Foreground);   
        }
    }
}