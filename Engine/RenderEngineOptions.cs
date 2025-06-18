namespace RawDraw.Engine;

public class RenderEngineOptions
{
    public required string FrameBufferDevice { get; init; }
    public required string InputDevice { get; init; }

    public bool ShowMetrics { get; init; }
    public bool HideConsoleCaret { get; init; }
}