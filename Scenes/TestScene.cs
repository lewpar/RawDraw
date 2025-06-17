using System.Drawing;

namespace RawDraw.Scenes;

public class TestScene : IScene
{
    private int cubeX;
    private int cubeY;

    private List<RawRectangle> rectangles;

    public TestScene(int width, int height)
    {
        cubeX = 300;
        cubeY = 300;

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
                Position = new Point(rand.Next(0, width - rectSize), rand.Next(0, height - rectSize)),
                Size = new Point(rectSize, rectSize),
                Velocity = new Point(rand.Next(0, 2) > 0 ? 1 : -1, rand.Next(0, 2) > 0 ? 1 : -1),
                Color = color
            });
        }
    }
    
    public void Render(FrameBuffer buffer, long deltaTime)
    {
        buffer.FillRect(400, 400, 100, 100, Color.Red);
        buffer.DrawText(401, 401, "Hello, World!", Color.White);

        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.RightArrow:
                    cubeX += 3;
                    break;

                case ConsoleKey.LeftArrow:
                    cubeX -= 3;
                    break;

                case ConsoleKey.UpArrow:
                    cubeY -= 3;
                    break;

                case ConsoleKey.DownArrow:
                    cubeY += 3;
                    break;
            }
        }

        buffer.DrawRect(cubeX, cubeY, 10, 10, 30, Color.Red);

        foreach (var rect in rectangles)
        {
            if (rect.Position.X + rect.Size.X >= buffer.Width)
            {
                rect.Velocity = new Point(-1, rect.Velocity.Y);
            }
            else if (rect.Position.X <= 0)
            {
                rect.Velocity = new Point(1, rect.Velocity.Y);
            }

            if (rect.Position.Y + rect.Size.Y >= buffer.Height)
            {
                rect.Velocity = new Point(rect.Velocity.X, -1);
            }
            else if (rect.Position.Y <= 0)
            {
                rect.Velocity = new Point(rect.Velocity.Y, 1);
            }

            rect.Position = new Point(rect.Position.X + rect.Velocity.X, rect.Position.Y);
            rect.Position = new Point(rect.Position.X, rect.Position.Y + rect.Velocity.Y);

            buffer.FillRect(rect.Position.X, rect.Position.Y, 10, 10, rect.Color);
        }
    }
}