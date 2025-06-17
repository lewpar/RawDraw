using System.Diagnostics;
using System.Drawing;
using System.IO.MemoryMappedFiles;

namespace RawDraw;

public class FrameBuffer : IDisposable
{
    public FrameBufferOptions Options { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private FrameBufferInfo _frameBufferInfo;

    private int _bytesPerPixel;

    private FileStream _frameBufferStream;
    private MemoryMappedFile _frameBufferMemoryMap;
    private MemoryMappedViewAccessor _frameBufferAccessor;

    private byte[] _softwareBackFrameBuffer;

    private long _timeSinceLastSwap;
    private Stopwatch _swapTimer;

    public FrameBuffer(FrameBufferOptions options)
    {
        Options = options;

        if (!File.Exists(options.Path))
        {
            throw new Exception($"Failed to find frame buffer at path '{options.Path}'.");
        }

        var frameBufferInfo = FrameBufferUtilities.GetFrameBufferInfo();
        if (frameBufferInfo is null)
        {
            throw new Exception("Failed to get frame buffer information.");
        }

        if (frameBufferInfo.Depth != 32)
        {
            throw new Exception($"Expected a color depth of 32-bit, but got '{frameBufferInfo.Depth}-bit'.");
        }

        _frameBufferInfo = frameBufferInfo;

        Width = frameBufferInfo.Width;
        Height = frameBufferInfo.Height;

        _bytesPerPixel = frameBufferInfo.Depth / 8;
        var frameBufferSize = frameBufferInfo.Width * frameBufferInfo.VirtualHeight * _bytesPerPixel;

        _frameBufferStream = new FileStream(options.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        _frameBufferMemoryMap = MemoryMappedFile.CreateFromFile(_frameBufferStream, null, frameBufferSize, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
        _frameBufferAccessor = _frameBufferMemoryMap.CreateViewAccessor(0, frameBufferSize, MemoryMappedFileAccess.Write);

        _softwareBackFrameBuffer = new byte[frameBufferInfo.Width * frameBufferInfo.Height * _bytesPerPixel];

        _timeSinceLastSwap = 0;
        _swapTimer = new Stopwatch();

        if (Options.HideCaret)
        {
            Console.Write("\x1b[?25l"); // Hide cursor
            AppDomain.CurrentDomain.ProcessExit += (_, _) => Console.Write("\x1b[?25h"); // Show cursor   
        }
    }

    public void Clear(Color color)
    {
        var rawColor = FrameBufferUtilities.ColorToLittleEndian(color.R, color.G, color.B);

        for (int i = 0; i < _softwareBackFrameBuffer.Length; i += _bytesPerPixel)
        {
            _softwareBackFrameBuffer[i] = rawColor[0];
            _softwareBackFrameBuffer[i + 1] = rawColor[1];
            _softwareBackFrameBuffer[i + 2] = rawColor[2];
            _softwareBackFrameBuffer[i + 3] = rawColor[3];
        }
    }

    public void DrawPixel(int x, int y, Color color)
    {
        if (x < 0 || x >= _frameBufferInfo.Width ||
            y < 0 || y >= _frameBufferInfo.Height)
        {
            return;
        }

        var rawColor = FrameBufferUtilities.ColorToLittleEndian(color.R, color.G, color.B);
        var pixelOffset = (y * _frameBufferInfo.Width + x) * _bytesPerPixel;

        Buffer.BlockCopy(rawColor, 0, _softwareBackFrameBuffer, pixelOffset, _bytesPerPixel);
    }

    public void DrawRect(int x, int y, int width, int height, int borderWidth, Color color)
    {
        if (borderWidth < 1)
        {
            return;
        }

        if (borderWidth > 10)
        {
            borderWidth = 10;
        }
        
        for (int i = 0; i < borderWidth; i++)
        {
            int topY = y + i;
            int bottomY = y + height - 1 - i;
            int leftX = x + i;
            int rightX = x + width - 1 - i;

            // Draw top horizontal line
            if (topY >= 0 && topY < _frameBufferInfo.Height)
            {
                for (int px = leftX; px <= rightX; px++)
                {
                    if (px < 0 || px >= _frameBufferInfo.Width) continue;
                    DrawPixel(px, topY, color);
                }
            }

            // Draw bottom horizontal line (if different from top)
            if (bottomY != topY && bottomY >= 0 && bottomY < _frameBufferInfo.Height)
            {
                for (int px = leftX; px <= rightX; px++)
                {
                    if (px < 0 || px >= _frameBufferInfo.Width) continue;
                    DrawPixel(px, bottomY, color);
                }
            }

            // Draw left vertical line
            if (leftX >= 0 && leftX < _frameBufferInfo.Width)
            {
                for (int py = topY; py <= bottomY; py++)
                {
                    if (py < 0 || py >= _frameBufferInfo.Height) continue;
                    DrawPixel(leftX, py, color);
                }
            }

            // Draw right vertical line (if different from left)
            if (rightX != leftX && rightX >= 0 && rightX < _frameBufferInfo.Width)
            {
                for (int py = topY; py <= bottomY; py++)
                {
                    if (py < 0 || py >= _frameBufferInfo.Height) continue;
                    DrawPixel(rightX, py, color);
                }
            }
        }
    }

    public void FillRect(int x, int y, int width, int height, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            if (py < 0 || py >= _frameBufferInfo.Height)
            {
                continue;
            }

            for (int px = x; px < x + width; px++)
            {
                if (px < 0 || px >= _frameBufferInfo.Width)
                {
                    continue;
                }

                DrawPixel(px, py, color);
            }
        }
    }
    public void FillRect(Rectangle rect, Color color)
    {
        FillRect(rect.X, rect.Y, rect.Width, rect.Height, color);
    }

    public void DrawChar(int x, int y, char character, Color color)
    {
        character = char.ToUpper(character);

        if (!FrameBufferFont.Basic8x8.TryGetValue(character, out var font))
        {
            font = FrameBufferFont.Basic8x8[' '];
        }

        for (int row = 0; row < 8; row++)
        {
            byte bits = font[row];
            for (int col = 0; col < 8; col++)
            {
                if ((bits & (1 << (7 - col))) != 0)
                {
                    DrawPixel(x + col, y + row, color);
                }
            }
        }
    }

    public void DrawText(int x, int y, string text, Color color)
    {
        for (int i = 0; i < text.Length; i++)
        {
            DrawChar(x + i * 8, y, text[i], color);
        }
    }

    private void DrawMetrics()
    {
        FillRect(5, 5, 175, 25, Color.Gray);
        DrawRect(5, 5, 175, 25, 3, Color.DarkGray);
        DrawText(15, 15, $"Frame Diff (ms): {_timeSinceLastSwap}", Color.White);
    }

    public void SwapBuffers()
    {
        if (Options.EnableMetrics)
        {
            _timeSinceLastSwap = _swapTimer.ElapsedMilliseconds;

            DrawMetrics();

           _swapTimer.Restart();
        }

        _frameBufferAccessor.WriteArray(0, _softwareBackFrameBuffer, 0, _softwareBackFrameBuffer.Length);
    }

    public void Dispose()
    {
        _frameBufferAccessor.Dispose();
        _frameBufferMemoryMap.Dispose();
        _frameBufferStream.Dispose();
    }
}