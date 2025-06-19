using RawDraw.Engine.Primitive;

namespace RawDraw.Engine.Input;

public class InputManager : IDisposable
{
    private KeyboardReader _keyReader;
    private MouseReader _mouseReader;
    private Task? _keyProcessingTask;
    private Task? _mouseProcessingTask;
    private CancellationTokenSource _cancellationTokenSource;

    public InputManager(RenderEngineOptions options)
    {
        _keyReader = new KeyboardReader(options.InputDevice);
        _mouseReader = new MouseReader(options.MouseDevice);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Initialize()
    {
        _keyReader.Initialize();
        _mouseReader.Initialize();

        _keyProcessingTask = Task.Run(ProcessKeyEventsAsync);
        _mouseProcessingTask = Task.Run(ProcessMouseEventsAsync);
    }

    private async Task ProcessKeyEventsAsync()
    {
        await _keyReader.ProcessKeyEventsAsync(_cancellationTokenSource.Token);
    }

    private async Task ProcessMouseEventsAsync()
    {
        await _mouseReader.ProcessMouseEventsAsync(_cancellationTokenSource.Token);
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

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _keyProcessingTask?.Wait();
        _cancellationTokenSource.Dispose();
    }
}
