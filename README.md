# Linux Raw Buffer Drawing
This code shows how you can access the raw frame buffer in a Linux environment to draw onto the screen.

## Usage
```cs
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
engine.SceneManager.Push(new PlatformerScene()
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
```

## Notes
- Linux user must be in the `video` and `input` user groups (requires relog).
    - `usermod -aG video $USER`
    - `usermod -aG input $USER`

## Dependencies
- `fbset` - Used to get the frame buffer information (size, color depth, etc..)