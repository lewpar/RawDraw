using RawDraw.Engine;
using RawDraw.Engine.Input;
using RawDraw.Scenes;

namespace RawDraw;

class Program
{
    static void Main(string[] args)
    {
        using var engine = new RenderEngine(new RenderEngineOptions()
        {
            FrameBufferDevice = "/dev/fb0",
            KeyboardDevice = InputDeviceEnumerator.AutoDetectKeyboardDevice(),
            ShowMetrics = true,
            HideConsoleCaret = true,
            MouseDevice = InputDeviceEnumerator.AutoDetectMouseDevice(),
            TouchDevice = InputDeviceEnumerator.AutoDetectTouchDevice(),
            MaxTouchX = 1452,
            MaxTouchY = 912
        });

        engine.Initialize();
        engine.SceneManager.Push(new PlatformerRenderScene()
        {
            UI = "UI/platformer.xml"
        });

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