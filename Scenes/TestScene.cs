using System.Drawing;

namespace RawDraw.Scenes;

public class TestScene : IScene
{
    private List<RawRectangle> rectangles;

    public TestScene(int width, int height)
    {
        rectangles = new List<RawRectangle>();

        var colors = new List<Color>()
        {
            Color.Green,
            Color.Purple,
            Color.Fuchsia,
            Color.Yellow,
            Color.SkyBlue
        };

        var rand = new Random();
        var rectSize = 10;

        for (int i = 0; i < 5000; i++)
        {
            var color = colors[rand.Next(0, colors.Count)];
            rectangles.Add(new RawRectangle()
            {
                Position = new PointF(rand.Next(0, width - rectSize), rand.Next(0, height - rectSize)),
                Size = new Point(rectSize, rectSize),
                Velocity = new PointF(rand.NextSingle() / 2, rand.NextSingle() / 2),
                Color = color
            });
        }
    }
    
    public void Render(FrameBuffer buffer, long deltaTime)
    {
        buffer.FillRect(400, 400, 100, 100, Color.Red);
        buffer.DrawText(401, 401, "Hello, World!", Color.White);

        foreach (var rect in rectangles)
        {
            if (rect.Position.X + rect.Size.X >= buffer.Width)
            {
                rect.Velocity = new PointF(-Math.Abs(rect.Velocity.X), rect.Velocity.Y);
            }
            else if (rect.Position.X <= 0)
            {
                rect.Velocity = new PointF(Math.Abs(rect.Velocity.X), rect.Velocity.Y);
            }

            if (rect.Position.Y + rect.Size.Y >= buffer.Height)
            {
                rect.Velocity = new PointF(rect.Velocity.X, -Math.Abs(rect.Velocity.Y));
            }
            else if (rect.Position.Y <= 0)
            {
                rect.Velocity = new PointF(rect.Velocity.X, Math.Abs(rect.Velocity.Y));
            }

            rect.Position = new PointF(
                rect.Position.X + rect.Velocity.X * deltaTime,
                rect.Position.Y + rect.Velocity.Y * deltaTime);

            buffer.FillRect((int)rect.Position.X, (int)rect.Position.Y, 10, 10, rect.Color);
        }
    }

}