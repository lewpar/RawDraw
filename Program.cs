using System.Drawing;

namespace RawDraw;

class Program
{
    static void Main(string[] args)
    {
        using var buffer = new FrameBuffer(new FrameBufferOptions
        {
            Path = "/dev/fb0",
            EnableMetrics = true
        });

        int cubeX = 300;
        int cubeY = 300;

        while (true)
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

            buffer.SwapBuffers();
        }
    }
}