using System.Drawing;
using System.IO.MemoryMappedFiles;

namespace RawDraw;

public class FrameBuffer : IDisposable
{
    public FrameBufferOptions Options { get; private set; }

    private FrameBufferInfo _frameBufferInfo;

    private int _bytesPerPixel;

    private FileStream _frameBufferStream;
    private MemoryMappedFile _frameBufferMemoryMap;
    private MemoryMappedViewAccessor _frameBufferAccessor;

    private byte[] _softwareBackFrameBuffer;

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

        _bytesPerPixel = frameBufferInfo.Depth / 8;
        var frameBufferSize = frameBufferInfo.Width * frameBufferInfo.VirtualHeight * _bytesPerPixel;

        _frameBufferStream = new FileStream(options.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        _frameBufferMemoryMap = MemoryMappedFile.CreateFromFile(_frameBufferStream, null, frameBufferSize, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
        _frameBufferAccessor = _frameBufferMemoryMap.CreateViewAccessor(0, frameBufferSize, MemoryMappedFileAccess.Write);

        _softwareBackFrameBuffer = new byte[frameBufferInfo.Width * frameBufferInfo.Height * _bytesPerPixel];
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

    public void SwapBuffers()
    {
        _frameBufferAccessor.WriteArray(0, _softwareBackFrameBuffer, 0, _softwareBackFrameBuffer.Length);
    }

    public void Dispose()
    {
        _frameBufferAccessor.Dispose();
        _frameBufferMemoryMap.Dispose();
        _frameBufferStream.Dispose();
    }
}