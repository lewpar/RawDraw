using System.Diagnostics;
using System.Text.RegularExpressions;

using RawDraw.Engine.Drawing;
using RawDraw.Engine.Input;
using RawDraw.Engine.Primitive;
using RawDraw.Engine.Scene;

namespace RawDraw.Engine;

public class RenderEngine : IDisposable
{
    public RenderEngineOptions RenderOptions { get => _renderOptions; }
    private RenderEngineOptions _renderOptions;

    public SceneManager SceneManager { get => _sceneManager; }
    private SceneManager _sceneManager;

    public InputManager InputManager { get => _inputManager; }
    private InputManager _inputManager;

    public FrameBuffer? FrameBuffer { get => _frameBuffer; }
    private FrameBuffer? _frameBuffer;

    public FrameBufferInfo? FrameBufferInfo { get => _frameBufferInfo; }
    private FrameBufferInfo? _frameBufferInfo;

    private long _deltaTimeMs;
    private Stopwatch _deltaTimer;

    private Vector2 _mouseCursorPosition;
    private Vector2 _touchCursorPosition;

    public RenderEngine(RenderEngineOptions renderOptions)
    {
        _renderOptions = renderOptions;
        _sceneManager = new SceneManager();
        _inputManager = new InputManager(renderOptions);
        _deltaTimer = new Stopwatch();
        _mouseCursorPosition = new Vector2(0, 0);
        _touchCursorPosition = new Vector2(0, 0);
    }

    public void Initialize()
    {
        if (!File.Exists(RenderOptions.FrameBufferDevice))
        {
            throw new Exception($"Failed to find frame buffer at path '{RenderOptions.FrameBufferDevice}'.");
        }

        if (!File.Exists(RenderOptions.InputDevice))
        {
            throw new Exception($"Failed to find input device at path '{RenderOptions.InputDevice}'.");
        }

        var frameBufferInfo = GetFrameBufferInfo();
        if (frameBufferInfo is null)
        {
            throw new Exception("Failed to get frame buffer information from 'fbset'.");
        }

        if (frameBufferInfo.Depth != 16 && frameBufferInfo.Depth != 32)
        {
            throw new Exception($"Unsupported color depth: {frameBufferInfo.Depth}-bit. Only 16-bit and 32-bit are supported.");
        }

        _frameBufferInfo = frameBufferInfo;
        _frameBuffer = new FrameBuffer(frameBufferInfo, _renderOptions);

        if (_renderOptions.HideConsoleCaret)
        {
            Console.Write("\x1b[?25l");
        }

        _inputManager.Initialize();

        _mouseCursorPosition = new Vector2(_frameBufferInfo.Width / 2, _frameBufferInfo.Height / 2);
    }

    public static FrameBufferInfo? GetFrameBufferInfo()
    {
        try
        {
            var startInfo = new ProcessStartInfo("fbset")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return ParseFrameBufferInfo(output);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get frame buffer information from 'fbset': {ex.Message}");
        }
    }

    private static FrameBufferInfo ParseFrameBufferInfo(string input)
    {
        var info = new FrameBufferInfo();

        var modeMatch = Regex.Match(input, @"mode\s+""(\d+x\d+)""");
        if (modeMatch.Success)
            info.Mode = modeMatch.Groups[1].Value;

        var geometryMatch = Regex.Match(input, @"geometry\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)");
        if (geometryMatch.Success)
        {
            info.Width = int.Parse(geometryMatch.Groups[1].Value);
            info.Height = int.Parse(geometryMatch.Groups[2].Value);
            info.VirtualWidth = int.Parse(geometryMatch.Groups[3].Value);
            info.VirtualHeight = int.Parse(geometryMatch.Groups[4].Value);
            info.Depth = int.Parse(geometryMatch.Groups[5].Value);
        }

        var timingsMatch = Regex.Match(input, @"timings\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)");
        if (timingsMatch.Success)
        {
            for (int i = 0; i < 7; i++)
            {
                info.Timings[i] = int.Parse(timingsMatch.Groups[i + 1].Value);
            }
        }

        var rgbaMatch = Regex.Match(input, @"rgba\s+(\d+)/(\d+),(\d+)/(\d+),(\d+)/(\d+),(\d+)/(\d+)");
        if (rgbaMatch.Success)
        {
            info.Rgba = new FrameBufferInfo.RgbaInfo
            {
                RedLength = int.Parse(rgbaMatch.Groups[1].Value),
                RedOffset = int.Parse(rgbaMatch.Groups[2].Value),

                GreenLength = int.Parse(rgbaMatch.Groups[3].Value),
                GreenOffset = int.Parse(rgbaMatch.Groups[4].Value),

                BlueLength = int.Parse(rgbaMatch.Groups[5].Value),
                BlueOffset = int.Parse(rgbaMatch.Groups[6].Value),

                AlphaLength = int.Parse(rgbaMatch.Groups[7].Value),
                AlphaOffset = int.Parse(rgbaMatch.Groups[8].Value),
            };
        }

        return info;
    }

    private void RenderMetrics()
    {
        if (_frameBuffer is null)
        {
            return;
        }
        
        _frameBuffer.DrawText(15, 15, $"Frame Diff (ms): {_deltaTimeMs}", Color.White);
    }

    private void UpdateMousePosition()
    {
        if (_frameBufferInfo is null)
        {
            return;
        }

        var mouseDelta = _inputManager.GetMouseDelta();

        var newMouseX = _mouseCursorPosition.x + mouseDelta.x;
        var newMouseY = _mouseCursorPosition.y + mouseDelta.y;

        if (newMouseX <= 0)
        {
            newMouseX = 0;
        }
        
        if (newMouseX >= _frameBufferInfo.Width)
        {
            newMouseX = _frameBufferInfo.Width;
        }

        if (newMouseY <= 0)
        {
            newMouseY = 0;
        }

        if (newMouseY >= _frameBufferInfo.Height)
        {
            newMouseY = _frameBufferInfo.Height;
        }

        _mouseCursorPosition = new Vector2(newMouseX, newMouseY);
    }

    private void RenderMouseCursor()
    {
        if (_frameBuffer is null)
        {
            return;
        }

        _frameBuffer.FillTriangle(new Vector2(_mouseCursorPosition.x, _mouseCursorPosition.y),
                                    new Vector2(_mouseCursorPosition.x + 8, _mouseCursorPosition.y + 5),
                                    new Vector2(_mouseCursorPosition.x + 2, _mouseCursorPosition.y + 10),
                                    Color.Red);
    }

    private void UpdateTouchPosition()
    {
        _touchCursorPosition = _inputManager.GetTouchPositionNormalized();
    }

    private void RenderTouchCursor()
    {
        if (_frameBuffer is null ||
            _frameBufferInfo is null)
        {
            return;
        }

        var isTouching = _inputManager.IsTouching();

        var x = _touchCursorPosition.x * _frameBufferInfo.Width;
        var y = _touchCursorPosition.y * _frameBufferInfo.Height;

        _frameBuffer.FillRect((int)x, (int)y, 10, 10, isTouching ? Color.Red : Color.Gray);
    }

    public void Update()
    {
        if (_frameBuffer is null)
        {
            throw new Exception("Frame buffer not initialized.");
        }

        if (SceneManager.CurrentScene is null)
        {
            throw new Exception("No scene available to render.");
        }

        if (SceneManager.CurrentScene.Input is null)
        {
            SceneManager.CurrentScene.Input = _inputManager;
        }

        // Consume any console keys so they dont get rendered.
#if !DEBUG
        if (Console.KeyAvailable)
        {
            _ = Console.ReadKey(true);
        }
#endif

        _deltaTimeMs = _deltaTimer.ElapsedMilliseconds;
        _deltaTimer.Restart();

        SceneManager.CurrentScene.Update(_deltaTimeMs);
        SceneManager.CurrentScene.Draw(_frameBuffer);

        if (_renderOptions.ShowMetrics)
        {
            RenderMetrics();
        }

        if (_renderOptions.ShowMouseCursor)
        {
            UpdateMousePosition();
            RenderMouseCursor();
        }

        if (_renderOptions.ShowTouchCursor)
        {
            UpdateTouchPosition();
            RenderTouchCursor();
        }

        _frameBuffer.SwapBuffers();
    }

    public void Dispose()
    {
        _inputManager?.Dispose();
        _frameBuffer?.Dispose();
    }
}