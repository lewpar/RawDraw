using System.Runtime.InteropServices;

namespace RawDraw.Engine.Input;

public class TouchReader
{
    private const ushort EV_ABS = 0x03;
    private const ushort EV_KEY = 0x01;
    private const ushort EV_SYN = 0x00;

    private const ushort ABS_X = 0x00;
    private const ushort ABS_Y = 0x01;
    private const ushort ABS_MT_POSITION_X = 0x35;
    private const ushort ABS_MT_POSITION_Y = 0x36;

    private const ushort BTN_TOUCH = 0x14a;

    private readonly string _devicePath;
    private readonly int _maxTouchX;
    private readonly int _maxTouchY;
    private FileStream? _deviceStream;
    private readonly object _lockObject = new();

    public int TouchX { get; private set; }
    public int TouchY { get; private set; }
    public bool IsTouching { get; private set; }

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

    public TouchReader(string devicePath, int maxTouchX, int maxTouchY)
    {
        _devicePath = devicePath;
        _maxTouchX = maxTouchX;
        _maxTouchY = maxTouchY;
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
            throw new Exception("Permission denied. Try running: sudo usermod -aG input $USER");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to open device: {ex.Message}");
        }
    }

    public async Task ProcessTouchEventsAsync(CancellationToken cancellationToken)
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

                    lock (_lockObject)
                    {
                        switch (inputEvent.Type)
                        {
                            case EV_ABS:
                                if (inputEvent.Code == ABS_X || inputEvent.Code == ABS_MT_POSITION_X)
                                    TouchX = inputEvent.Value;
                                else if (inputEvent.Code == ABS_Y || inputEvent.Code == ABS_MT_POSITION_Y)
                                    TouchY = inputEvent.Value;
                                break;

                            case EV_KEY:
                                if (inputEvent.Code == BTN_TOUCH)
                                    IsTouching = inputEvent.Value != 0;
                                break;

                            case EV_SYN:
                                // Touch frame complete
                                break;
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

    public (float normX, float normY, bool isTouching) GetTouchState()
    {
        lock (_lockObject)
        {
            float normX = Math.Clamp((float)TouchX / _maxTouchX, 0f, 1f);
            float normY = Math.Clamp((float)TouchY / _maxTouchY, 0f, 1f);
            return (normX, normY, IsTouching);
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
