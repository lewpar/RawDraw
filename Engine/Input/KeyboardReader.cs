using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace RawDraw.Engine.Input;

public class KeyboardReader
{
    private const ushort EV_KEY = 0x01;
    private readonly string _devicePath;
    private FileStream? _deviceStream;
    private readonly BlockingCollection<InputEvent> _eventQueue = new();

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

    public async Task ProcessKeyEventsAsync()
    {
        var buffer = new byte[Marshal.SizeOf(typeof(InputEvent))];

        if (_deviceStream is null)
        {
            return;
        }

        while (true)
        {
            try
            {
                int bytesRead = await _deviceStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == buffer.Length)
                {
                    var inputEvent = ByteArrayToStructure<InputEvent>(buffer);
                    _eventQueue.Add(inputEvent);
                }
            }
            catch
            {
                break;
            }
        }
    }

    public bool TryReadNextEvent(out InputEvent inputEvent)
    {
        return _eventQueue.TryTake(out inputEvent);
    }

    public bool TryGetKeyEvent(out ushort keyCode, out bool pressed)
    {
        keyCode = 0;
        pressed = false;

        if (TryReadNextEvent(out var ev))
        {
            if (ev.Type == EV_KEY && (ev.Value == 0 || ev.Value == 1))
            {
                keyCode = ev.Code;
                pressed = ev.Value == 1;
                return true;
            }
        }

        return false;
    }

    private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        T structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        handle.Free();
        return structure;
    }
}
