using System.Runtime.InteropServices;

namespace RawDraw.Engine.Input;

public class MouseReader
{
    private const ushort EV_REL = 0x02;
    private const ushort EV_KEY = 0x01;
    private const ushort EV_SYN = 0x00;

    private const ushort REL_X = 0x00;
    private const ushort REL_Y = 0x01;
    private const ushort REL_WHEEL = 0x08;

    private const ushort BTN_LEFT = 0x110;
    private const ushort BTN_RIGHT = 0x111;
    private const ushort BTN_MIDDLE = 0x112;

    private readonly string _devicePath;
    private FileStream? _deviceStream;
    private readonly object _lockObject = new();

    public int DeltaX { get; private set; }
    public int DeltaY { get; private set; }
    public int WheelDelta { get; private set; }
    public bool LeftDown { get; private set; }
    public bool RightDown { get; private set; }
    public bool MiddleDown { get; private set; }

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

    public MouseReader(string devicePath)
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

    public async Task ProcessMouseEventsAsync(CancellationToken cancellationToken)
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
                            case EV_REL:
                                if (inputEvent.Code == REL_X)
                                    DeltaX += inputEvent.Value;
                                else if (inputEvent.Code == REL_Y)
                                    DeltaY += inputEvent.Value;
                                else if (inputEvent.Code == REL_WHEEL)
                                    WheelDelta += inputEvent.Value;
                                break;

                            case EV_KEY:
                                switch (inputEvent.Code)
                                {
                                    case BTN_LEFT:
                                        LeftDown = inputEvent.Value != 0;
                                        break;
                                    case BTN_RIGHT:
                                        RightDown = inputEvent.Value != 0;
                                        break;
                                    case BTN_MIDDLE:
                                        MiddleDown = inputEvent.Value != 0;
                                        break;
                                }
                                break;

                            case EV_SYN:
                                // Sync event - could be used to batch input frame, if needed
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

    public (int dx, int dy, int wheel) GetAndResetDeltas()
    {
        lock (_lockObject)
        {
            var result = (DeltaX, DeltaY, WheelDelta);
            DeltaX = 0;
            DeltaY = 0;
            WheelDelta = 0;
            return result;
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
