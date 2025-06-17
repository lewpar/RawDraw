using System.Drawing;
using System.Numerics;

namespace RawDraw;

class Program
{
    private static List<RawRectangle>? rectangles;

    static void Main(string[] args)
    {
        using var buffer = new FrameBuffer(new FrameBufferOptions
        {
            Path = "/dev/fb0",
            EnableMetrics = true
        });

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
                Position = new Point(rand.Next(0, buffer.Width - rectSize), rand.Next(0, buffer.Height - rectSize)),
                Size = new Point(rectSize, rectSize),
                Velocity = new Point(rand.Next(0, 2) > 0 ? 1 : -1, rand.Next(0, 2) > 0 ? 1 : -1),
                Color = color
            });
        }

        int cubeX = 300;
        int cubeY = 300;

        while (true)
        {
            buffer.Clear(Color.Black);

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

            buffer.SwapBuffers();
        }
    }
}