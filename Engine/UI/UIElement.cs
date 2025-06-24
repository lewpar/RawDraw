using RawDraw.Engine.Drawing;
using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.UI;

public abstract class UIElement
{
    public Guid Id { get; init; }

    public Vector2 Position { get; init; }

    public Vector2 Size { get; init; }
    public Rectangle Bounds { get; init; }

    public UIElement(int x, int y, int width, int height)
    {
        Id = Guid.NewGuid();
        Position = new Vector2(x, y);
        Size = new Vector2(width, height);
        Bounds = new Rectangle(x, y, width, height);
    }

    public virtual void Draw(FrameBuffer buffer) { }
    public virtual void Update(float deltaTimeMs) { }
} 