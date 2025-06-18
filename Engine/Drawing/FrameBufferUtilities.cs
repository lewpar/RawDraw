namespace RawDraw.Engine.Drawing;

public class FrameBufferUtilities
{
    public static byte[] ColorToLittleEndian(byte r, byte g, byte b)
    {
        return new byte[] { b, g, r, 255 /* UNUSED ALPHA CHANNEL */ };
    }

    public static byte[] ColorTo16Bit(byte r, byte g, byte b)
    {
        // Convert 8-bit RGB to 16-bit RGB565 format
        // R: 5 bits (0-31), G: 6 bits (0-63), B: 5 bits (0-31)
        ushort red = (ushort)((r >> 3) & 0x1F);   // 5 bits
        ushort green = (ushort)((g >> 2) & 0x3F); // 6 bits  
        ushort blue = (ushort)((b >> 3) & 0x1F);  // 5 bits

        // Pack into 16-bit value: RRRRRGGGGGGBBBBB
        ushort color16 = (ushort)((red << 11) | (green << 5) | blue);

        // Convert to little-endian bytes
        return new byte[] { (byte)(color16 & 0xFF), (byte)((color16 >> 8) & 0xFF) };
    }
}