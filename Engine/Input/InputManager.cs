using System.Collections.Concurrent;

namespace RawDraw.Engine.Input;

public class InputManager
{
    private KeyboardReader _keyReader;
    private readonly HashSet<KeyCodes> _keysDown = new();

    public InputManager(RenderEngineOptions options)
    {
        _keyReader = new KeyboardReader(options.InputDevice);
    }

    public void Initialize()
    {
        _keyReader.Initialize();

        _ = Task.Run(ProcessKeyEventsAsync);
    }

    private async Task ProcessKeyEventsAsync()
    {
        while (true)
        {
            while (_keyReader.TryGetKeyEvent(out ushort code, out bool pressed))
            {
                var key = (KeyCodes)code;
                lock (_keysDown)
                {
                    if (pressed)
                        _keysDown.Add(key);
                    else
                        _keysDown.Remove(key);
                }
            }

            await Task.Delay(1);
        }
    }

    public bool IsKeyDown(KeyCodes keyCode)
    {
        lock (_keysDown)
        {
            return _keysDown.Contains(keyCode);
        }
    }
}
