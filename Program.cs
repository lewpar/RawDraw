using System.Diagnostics;
using System.Drawing;

namespace RawDraw;

class Program
{
    static async Task Main(string[] args)
    {
        using var buffer = new FrameBuffer(new FrameBufferOptions
        {
            Path = "/dev/fb0",
            EnableMetrics = true
        });

        while (true)
        {
            buffer.FillRect(400, 400, 100, 100, Color.Red);
            buffer.DrawText(401, 401, "Hello, World!", Color.White);

            buffer.SwapBuffers();
        }
    }
}