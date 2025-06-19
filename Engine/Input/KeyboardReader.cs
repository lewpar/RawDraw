using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace RawDraw.Engine.Input;

public class KeyboardReader
{
    private const ushort EV_KEY = 0x01;
    private readonly string _devicePath;
    private FileStream? _deviceStream;
    private readonly HashSet<KeyCodes> _keysDown = new();
    private readonly object _lockObject = new();

    [StructLayout(LayoutKind.Sequential)]
    public struct InputEvent
    {
        public TimeVal Time;
        public ushort Type;
        public ushort Code;
        public int Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TimeVal
    {
        public long TvSec;
        public long TvUsec;
    }

    public KeyboardReader(string devicePath)
    {
        _devicePath = devicePath;
    }

    public void Initialize()
    {
        if (!File.Exists(_devicePath))
            throw new Exception($"Device not found: {_devicePath}");

        try
        {
            _deviceStream = new FileStream(
                _devicePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
        }
        catch (UnauthorizedAccessException)
        {
            throw new Exception($"Permission denied. Try running: sudo usermod -aG input $USER");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to open device: {ex.Message}");
        }
    }

    public async Task ProcessKeyEventsAsync(CancellationToken cancellationToken)
    {
        if (_deviceStream is null)
            throw new InvalidOperationException("Device not initialized.");

        var buffer = new byte[Marshal.SizeOf(typeof(InputEvent))];

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                int bytesRead = await _deviceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == buffer.Length)
                {
                    var inputEvent = ByteArrayToStructure<InputEvent>(buffer);
                    if (inputEvent.Type == EV_KEY)
                    {
                        var keyCode = (KeyCodes)inputEvent.Code;
                        var isPressed = inputEvent.Value > 0;

                        lock (_lockObject)
                        {
                            if (isPressed)
                                _keysDown.Add(keyCode);
                            else
                                _keysDown.Remove(keyCode);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                break;
            }
        }
    }

    public bool IsKeyDown(KeyCodes keyCode)
    {
        lock (_lockObject)
        {
            return _keysDown.Contains(keyCode);
        }
    }

    private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        handle.Free();
        return structure;
    }
}
