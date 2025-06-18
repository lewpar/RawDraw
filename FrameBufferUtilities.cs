using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RawDraw;

public static class FrameBufferUtilities
{
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
            throw new Exception($"Failed to read resolution from fbset: {ex.Message}");
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