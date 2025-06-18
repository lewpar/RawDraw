using System.Collections.Concurrent;

namespace RawDraw.Engine.Input;

public class InputManager : IDisposable
{
    private KeyboardReader _keyReader;
    private Task? _keyProcessingTask;
    private CancellationTokenSource _cancellationTokenSource;

    public InputManager(RenderEngineOptions options)
    {
        _keyReader = new KeyboardReader(options.InputDevice);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Initialize()
    {
        _keyReader.Initialize();
        _keyProcessingTask = Task.Run(ProcessKeyEventsAsync);
    }

    private async Task ProcessKeyEventsAsync()
    {
        await _keyReader.ProcessKeyEventsAsync(_cancellationTokenSource.Token);
    }

    public bool IsKeyDown(KeyCodes keyCode)
    {
        return _keyReader.IsKeyDown(keyCode);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _keyProcessingTask?.Wait();
        _cancellationTokenSource.Dispose();
    }
}
