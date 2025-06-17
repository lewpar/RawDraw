using System.Drawing;
using System.Numerics;
using RawDraw.Scenes;

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

        var testScene = new TestScene(buffer.Width, buffer.Height);

        while (true)
        {
            buffer.Clear(Color.Black);

            testScene.Render(buffer);

            buffer.SwapBuffers();
        }
    }
}