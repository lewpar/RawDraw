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
            EnableMetrics = true,
            HideCaret = true
        });

        var testScene = new TestScene(buffer.Width, buffer.Height);
        var natureScene = new NatureScene();
        var spaceScene = new SpaceStationScene();

        while (true)
        {
            buffer.Clear(Color.Black);

            testScene.Render(buffer, buffer.DeltaTime);
            //natureScene.Render(buffer);
            //spaceScene.Render(buffer, buffer.DeltaTime);

            buffer.SwapBuffers();
        }
    }
}