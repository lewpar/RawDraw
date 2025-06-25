namespace RawDraw.Engine.Primitive;

public struct Rectangle
{
    public int x, y, width, height;

    public Rectangle(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public bool Contains(int px, int py)
    {
        return px >= x && px < x + width && py >= y && py < y + height;
    }
}