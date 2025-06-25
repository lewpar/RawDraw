using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.Input;

public class InputManager : IDisposable
{
    public static InputManager? Instance { get; set; }
    
    private static int _screenWidth;
    private static int _screenHeight;
    
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

        if (Instance is null)
        {
            Instance = this;
        }
    }

    public void Initialize(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        
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
    
    public static bool IsTouching(Rectangle region)
    {
        if (Instance is null)
        {
            throw new Exception("Input not initialized.");
        }

        var (normalizedX, normalizedY, isTouching) = Instance.GetTouchState();
        if (!isTouching)
        {
            return false;
        }

        var x = normalizedX * _screenWidth;
        var y = normalizedY * _screenHeight;

        if (x >= region.x && x <= (region.x + region.width) &&
            y >= region.y && y <= (region.y + region.height))
        {
            return true;
        }
        
        return false;
    }

    public void Dispose()
    {
        _keyReader?.StopEventLoop();
        _mouseReader?.StopEventLoop();
        _touchReader?.StopEventLoop();
    }
}
