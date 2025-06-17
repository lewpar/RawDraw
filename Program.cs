using System.Drawing;

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
        var cubeScene = new CubeScene(buffer.Width, buffer.Height);
        var gameScene = new TestGameScene(buffer.Width, buffer.Height);
        var spaceGameScene = new SpaceGameScene(buffer.Width, buffer.Height);

        while (true)
        {
            buffer.Clear(Color.Black);

            //testScene.Render(buffer, buffer.DeltaTime);
            //natureScene.Render(buffer);
            //spaceScene.Render(buffer, buffer.DeltaTime);
            //cubeScene.Render(buffer, buffer.DeltaTime);
            //gameScene.Render(buffer, buffer.DeltaTime);
            spaceGameScene.Render(buffer, buffer.DeltaTime);

            buffer.SwapBuffers();
        }
    }
}