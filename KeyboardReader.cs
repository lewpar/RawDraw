using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RawDraw;

public class KeyboardReader
{
    private const ushort EV_KEY = 0x01;
    private readonly string _devicePath;
    private FileStream? _deviceStream;

    [StructLayout(LayoutKind.Sequential)]
    struct InputEvent
    {
        public TimeVal Time;
        public ushort Type;
        public ushort Code;
        public int Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct TimeVal
    {
        public long TvSec;
        public long TvUsec;
    }

    public KeyboardReader(string devicePath)
    {
        _devicePath = devicePath;
    }

    public bool TryInitialize()
    {
        if (!File.Exists(_devicePath))
        {
            throw new FileNotFoundException($"Input device not found: {_devicePath}");
            return false;
        }

        try
        {
            _deviceStream = new FileStream(_devicePath, FileMode.Open, FileAccess.Read);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException($"Permission denied to access input device. Try running: sudo usermod -aG input $USER");
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to open input device: {ex.Message}", ex);
        }
    }

    public void Listen(Action<int, bool> onKeyChanged)
    {
        if (_deviceStream == null)
            throw new InvalidOperationException("Device not initialized.");

        byte[] buffer = new byte[Marshal.SizeOf(typeof(InputEvent))];

        while (true)
        {
            int bytesRead = _deviceStream.Read(buffer, 0, buffer.Length);
            if (bytesRead != buffer.Length)
                continue;

            InputEvent ev = ByteArrayToStructure<InputEvent>(buffer);

            if (ev.Type == EV_KEY)
            {
                if (ev.Value == 1)
                    onKeyChanged(ev.Code, true);
                else if (ev.Value == 0)
                    onKeyChanged(ev.Code, false);
            }
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
