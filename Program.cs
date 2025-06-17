using System.Drawing;

namespace RawDraw;

class Program
{
    static async Task Main(string[] args)
    {
        using var buffer = new FrameBuffer(new FrameBufferOptions
        {
            Path = "/dev/fb0"
        });

        while (true)
        {
            buffer.FillRect(new Rectangle(0, 0, 100, 100), Color.Red);
            buffer.DrawText(1, 1, "Hello, World!", Color.White);

            buffer.SwapBuffers();

            await Task.Delay(1);
        }
    }
}