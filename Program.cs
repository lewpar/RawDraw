using System.Drawing;
using RawDraw.Engine;
using RawDraw.Scenes;

namespace RawDraw;

class Program
{
    static void Main(string[] args)
    {
        using var engine = new RenderEngine(new RenderEngineOptions()
        {
            FrameBufferDevice = "/dev/fb0",
            InputDevice = "/dev/input/event3",
            ShowMetrics = true,
            HideConsoleCaret = true
        });

        engine.Initialize();
        engine.SceneManager.Push(new PlatformerScene());

        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Environment.Exit(0);
        };

        while (true)
        {
            engine.Update();
        }
    }
}