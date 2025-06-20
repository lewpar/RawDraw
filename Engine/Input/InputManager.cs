using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.Input;

public class InputManager : IDisposable
{
    private KeyboardReader _keyReader;
    private MouseReader _mouseReader;
    private TouchReader _touchReader;
    private Task? _keyProcessingTask;
    private Task? _mouseProcessingTask;
    private Task? _touchProcessingTask;
    private CancellationTokenSource _cancellationTokenSource;

    public InputManager(RenderEngineOptions options)
    {
        _keyReader = new KeyboardReader(options.InputDevice);
        _mouseReader = new MouseReader(options.MouseDevice);
        _touchReader = new TouchReader(options.TouchDevice, options.MaxTouchX, options.MaxTouchY);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Initialize()
    {
        _keyReader.Initialize();
        _mouseReader.Initialize();
        _touchReader.Initialize();

        _keyProcessingTask = Task.Run(ProcessKeyEventsAsync);
        _mouseProcessingTask = Task.Run(ProcessMouseEventsAsync);
        _touchProcessingTask = Task.Run(ProcessTouchEventsAsync);
    }

    private async Task ProcessKeyEventsAsync()
    {
        await _keyReader.ProcessKeyEventsAsync(_cancellationTokenSource.Token);
    }

    private async Task ProcessMouseEventsAsync()
    {
        await _mouseReader.ProcessMouseEventsAsync(_cancellationTokenSource.Token);
    }

    private async Task ProcessTouchEventsAsync()
    {
        await _touchReader.ProcessTouchEventsAsync(_cancellationTokenSource.Token);
    }

    public bool IsKeyDown(KeyCodes keyCode)
    {
        return _keyReader.IsKeyDown(keyCode);
    }

    public Vector2 GetMouseDelta()
    {
        var (dx, dy, _) = _mouseReader.GetAndResetDeltas();

        return new Vector2(dx, dy);
    }

    public bool IsTouching()
    {
        var (_, _, isTouching) = _touchReader.GetTouchState();

        return isTouching;
    }

    public Vector2 GetTouchPositionNormalized()
    {
        var (x, y, _) = _touchReader.GetTouchState();

        return new Vector2(x, y);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _keyProcessingTask?.Wait();
        _cancellationTokenSource.Dispose();
    }
}
