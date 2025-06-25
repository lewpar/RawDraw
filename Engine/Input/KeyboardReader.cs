using System.Runtime.InteropServices;

namespace RawDraw.Engine.Input;

public class KeyboardReader : LinuxInputDeviceReader<KeyboardReader.InputEvent>
{
    private const ushort EV_KEY = 0x01;
    private readonly HashSet<KeyCodes> _keysDown = new();

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

    public KeyboardReader(string devicePath) : base(devicePath) { }

    protected override void OnInputEvent(InputEvent inputEvent)
    {
        if (inputEvent.Type == EV_KEY)
        {
            var keyCode = (KeyCodes)inputEvent.Code;
            var isPressed = inputEvent.Value > 0;
            lock (LockObject)
            {
                if (isPressed)
                    _keysDown.Add(keyCode);
                else
                    _keysDown.Remove(keyCode);
            }
        }
    }

    public bool IsKeyDown(KeyCodes keyCode)
    {
        lock (LockObject)
        {
            return _keysDown.Contains(keyCode);
        }
    }
}
