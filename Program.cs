using System.Drawing;

using RawDraw.Scenes;

namespace RawDraw;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Frame Buffer (/dev/fb0): ");
        var frameBufferDevice = Console.ReadLine();

        Console.Write("Input Device (/dev/input/event3): ");
        var inputDevice = Console.ReadLine();

        using var buffer = new FrameBuffer(new FrameBufferOptions
        {
            Path = frameBufferDevice ?? "/dev/fb0",
            EnableMetrics = false,
            HideCaret = true
        });

        var testScene = new TestScene(buffer.Width, buffer.Height);
        var natureScene = new NatureScene();
        var spaceScene = new SpaceStationScene();
        var cubeScene = new CubeScene(buffer.Width, buffer.Height);
        var gameScene = new TestGameScene(buffer.Width, buffer.Height);
        var spaceGameScene = new SpaceGameScene(buffer.Width, buffer.Height);
        var platformerScene = new PlatformerScene(buffer.Width, buffer.Height);

        IScene currentScene = platformerScene;

        var reader = new KeyboardReader(inputDevice ?? "/dev/input/event3");

        if (!reader.TryInitialize())
        {
            Console.WriteLine("Failed to initialize keyboard reader.");
            return;
        }

        Task.Run(() => 
            reader.Listen((keyCode, state) => currentScene.Input(keyCode, state))
        );
        
        while (true)
        {
            if(Console.KeyAvailable)
            {
                _ = Console.ReadKey(true); // Dispose of any keys in the input buffer so they dont get rendered over the scene
            }

            buffer.Clear(Color.Black);

            currentScene.Render(buffer, buffer.DeltaTime);

            buffer.SwapBuffers();
        }
    }
}