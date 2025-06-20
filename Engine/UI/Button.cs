using RawDraw.Engine.Drawing;
using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.UI;

public class Button : IUIElement
{
    public string Id { get; }
    public Rectangle Bounds { get; set; }
    public string Label { get; set; }
    public Action? OnPressed { get; set; }
    public bool IsPressed { get; private set; }

    public Button(string id, Rectangle bounds, string label, Action? onPressed = null)
    {
        Id = id;
        Bounds = bounds;
        Label = label;
        OnPressed = onPressed;
    }

    public void Draw(FrameBuffer buffer)
    {
        var color = IsPressed ? Color.Gray : Color.White;
        buffer.FillRect(Bounds, color);
        buffer.DrawText(Bounds.x + 8, Bounds.y + Bounds.height / 2 - 4, Label, Color.Black);
    }

    public void UpdateTouch(float normX, float normY, bool isTouching, int screenWidth, int screenHeight)
    {
        int px = (int)(normX * screenWidth);
        int py = (int)(normY * screenHeight);
        bool inside = Bounds.Contains(px, py);
        if (isTouching && inside && !IsPressed)
        {
            IsPressed = true;
        }
        else if (!isTouching)
        {
            IsPressed = false;
        }
    }
} 