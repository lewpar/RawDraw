namespace RawDraw.Engine.Input;

public class InputManager : IDisposable
{
    private KeyboardReader _keyReader;
    private MouseReader? _mouseReader;
    private TouchReader? _touchReader;

    public InputManager(RenderEngineOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.KeyboardDevice))
        {
            throw new Exception("Keyboard device not specified.");
        }

        if (string.IsNullOrWhiteSpace(options.MouseDevice))
        {
            throw new Exception("Mouse device not specified.");
        }

        if (string.IsNullOrWhiteSpace(options.TouchDevice))
        {
            throw new Exception("Touch device not specified.");
        }

        _keyReader = new KeyboardReader(options.KeyboardDevice);
        _mouseReader = new MouseReader(options.MouseDevice);
        _touchReader = new TouchReader(options.TouchDevice, options.MaxTouchX, options.MaxTouchY);
    }

    public void Initialize()
    {
        _keyReader.Initialize();
        _keyReader.StartEventLoop();

        _mouseReader?.Initialize();
        _mouseReader?.StartEventLoop();
        
        _touchReader?.Initialize();
        _touchReader?.StartEventLoop();
    }

    public bool IsKeyDown(KeyCodes keyCode)
    {
        return _keyReader.IsKeyDown(keyCode);
    }

    public (int dx, int dy, int wheel) GetMouseDelta()
    {
        return _mouseReader?.GetAndResetDeltas() ?? (0, 0, 0);
    }

    public (float normX, float normY, bool isTouching) GetTouchState()
    {
        return _touchReader?.GetTouchState() ?? (0, 0, false);
    }

    public void Dispose()
    {
        _keyReader?.StopEventLoop();
        _mouseReader?.StopEventLoop();
        _touchReader?.StopEventLoop();
    }
}
